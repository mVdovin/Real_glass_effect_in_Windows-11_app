sampler2D input : register(s0);

// Параметры из C#
float EdgeWidth : register(c0);
float Distortion : register(c1);
float ReflectPower : register(c2);
float RGBShift : register(c3);
float ObjWidth : register(c4);
float ObjHeight : register(c5);
float ShapeType : register(c6);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    // Оригинальная логика масштаба
    float currentWidth = (ShapeType < 0.5) ? (ObjWidth / 4.0) : ObjWidth;
    float2 m = uv - 0.5;
    float aspect = currentWidth / (ObjHeight + 0.0001);

    // 1. Расчет формы (SDF) - твой оригинал
    float shape = 0;
    if (ShapeType < 0.5)
    {
        shape = pow(abs(m.x * 2.0), 10.0) + pow(abs(m.y * 2.0), 10.0);
    }
    else
    {
        float2 distVec = m * 2.0 * float2(aspect, 1.0);
        shape = pow(length(distVec), 6.0);
    }
    
    float mask = saturate(shape / max(0.001, EdgeWidth));
    float2 dir = normalize(m * float2(aspect, 1.0) + 1e-6);
    
    // --- ДОРАБОТКА: Стягивание вдоль левой и правой стороны ---
    // Вычисляем силу горизонтального "втягивания"
    // Чем ближе к левому/правому краю (abs(m.x) -> 0.5), тем сильнее эффект
    float sidePull = m.x * Distortion * 0.2;
    
    // 3. Расчет искаженных координат
    float totalPower = (ReflectPower + Distortion) * shape;
    
    // В targetUV добавляем sidePull по оси X
    float2 targetUV = uv - (dir * totalPower * 0.1);
    targetUV.x += sidePull * mask; // Применяем стягивание только в зоне фаски
    
    // Зеркальное ограничение
    float2 finalUV = lerp(uv, clamp(targetUV, 0.001, 0.999), mask);

    // 4. Хроматическая аберрация - твой оригинал
    float2 shift = dir * RGBShift * mask;
    
    float4 rCol = tex2D(input, clamp(finalUV + shift, 0.001, 0.999));
    float4 gCol = tex2D(input, clamp(finalUV, 0.001, 0.999));
    float4 bCol = tex2D(input, clamp(finalUV - shift, 0.001, 0.999));

    float4 result = float4(rCol.r, gCol.g, bCol.b, gCol.a);
    result.a = saturate(result.a * 100.0);

    // Легкое притенение
    float edgeDarkening = 1.0 - pow(mask, 4.0) * 0.1;

    return result * edgeDarkening;
}
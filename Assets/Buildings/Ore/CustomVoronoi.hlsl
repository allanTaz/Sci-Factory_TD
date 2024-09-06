inline float2 randomVector(float2 UV, float offset)
{
    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
    UV = frac(sin(mul(UV, m)) * 46839.32);
    return float2(sin(UV.y * +offset) * 0.5 + 0.5, cos(UV.x * offset) * 0.5 + 0.5);
}

void CustomVoronoi_float(float2 UV, float AngleOffset, float CellDensity, out float DistFromCenter, out float DistFromEdge, out float Cells)
{
    float2 cell = floor(UV * CellDensity);
    float2 posInCell = frac(UV * CellDensity);
    DistFromCenter = 8.0;
    float2 closestOffset;
    float cellColor = 0;
    
    for (int y = -1; y <= 1; ++y)
    {
        for (int x = -1; x <= 1; ++x)
        {
            float2 cellToCheck = float2(x, y);
            float2 cellOffset = cellToCheck - posInCell + randomVector(cell + cellToCheck, AngleOffset);
            float distToPoint = dot(cellOffset, cellOffset);
            if (distToPoint < DistFromCenter)
            {
                DistFromCenter = distToPoint;
                closestOffset = cellOffset;
                
                // Determine cell color based on its vertical and horizontal position
                float2 cellPosition = (cell + cellToCheck) / CellDensity;
                float verticalGradient = cellPosition.y;
                float horizontalGradient = cellPosition.x;
                cellColor = (verticalGradient + horizontalGradient) * 0.5; // Average of both gradients
            }
        }
    }
    
    DistFromEdge = 8.0;
    for (int y = -1; y <= 1; ++y)
    {
        for (int x = -1; x <= 1; ++x)
        {
            float2 cellToCheck = float2(x, y);
            float2 cellOffset = cellToCheck - posInCell + randomVector(cell + cellToCheck, AngleOffset);
            float2 diff = cellOffset - closestOffset;
            float2 normalizedDiff = normalize(diff);
            float distToEdge = dot(0.5 * (cellOffset + closestOffset), normalizedDiff);
            DistFromEdge = min(DistFromEdge, distToEdge);
        }
    }
    
    DistFromCenter = sqrt(DistFromCenter);
    DistFromEdge = sqrt(DistFromEdge);
    
    // Set Cells output to the calculated cell color
    Cells = cellColor;
}
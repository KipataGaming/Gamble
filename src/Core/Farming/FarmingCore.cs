using Godot;
using System;
using System.Collections.Generic;

namespace Game.Core
{
    public class FarmingCore
    {
        public event Action<Vector2I, SoilState> OnSoilStateChanged;
        public event Action<Vector2I, GrowthStage, string> OnCropStateChanged;

        private Dictionary<Vector2I, FarmPlotData> _grid = new Dictionary<Vector2I, FarmPlotData>();

        public void InitializeGrid(int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var coord = new Vector2I(x, y);
                    _grid[coord] = new FarmPlotData(coord);
                }
            }
        }

        public void InteractWithPlot(Vector2I coordinate, string actionType, string cropId = "")
        {
            if (!_grid.TryGetValue(coordinate, out FarmPlotData plot)) return;

            switch (actionType)
            {
                case "Till":
                    if (plot.TryTill()) OnSoilStateChanged?.Invoke(coordinate, plot.Soil);
                    break;
                case "Water":
                    if (plot.TryWater()) OnSoilStateChanged?.Invoke(coordinate, plot.Soil);
                    break;
                case "Plant":
                    if (plot.TryPlant(cropId)) OnCropStateChanged?.Invoke(coordinate, plot.Crop, plot.CurrentCropId);
                    break;
                case "Harvest":
                    if (plot.TryHarvest()) 
                    {
                        OnCropStateChanged?.Invoke(coordinate, plot.Crop, plot.CurrentCropId);
                        GD.Print($"[FarmingCore] Harvest successful at {coordinate}"); // Added for debugging
                    }
                    else
                    {
                        GD.Print($"[FarmingCore] Harvest failed. Is the crop mature?"); // Added for debugging
                    }
                    break;
                    
            }
        }

        public void SimulateDayPassing()
        {
            foreach (var kvp in _grid)
            {
                Vector2I coord = kvp.Key;
                FarmPlotData plot = kvp.Value;

                if (plot.ProcessDailyGrowth()) 
                    OnCropStateChanged?.Invoke(coord, plot.Crop, plot.CurrentCropId);
                
                if (plot.ProcessDailyDrying()) 
                    OnSoilStateChanged?.Invoke(coord, plot.Soil);
            }
        }
    }
}
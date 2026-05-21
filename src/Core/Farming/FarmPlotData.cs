using Godot;
using System;

namespace Game.Core
{
    public class FarmPlotData
    {
        public Vector2I Coordinate { get; }
        public SoilState Soil { get; private set; }
        public GrowthStage Crop { get; private set; }
        public string CurrentCropId { get; private set; }

        public FarmPlotData(Vector2I coordinate)
        {
            Coordinate = coordinate;
            Soil = SoilState.Untilled;
            Crop = GrowthStage.Empty;
            CurrentCropId = string.Empty;
        }

        // Gatekeeper methods to protect data integrity
        public void UpdateCropState(GrowthStage newCrop, string cropId)
        {
            Crop = newCrop;
            CurrentCropId = cropId;
        }

        public void UpdateSoilState(SoilState newSoil)
        {
            Soil = newSoil;
        }

        // Logic methods
        public bool TryTill()
        {
            if (Soil != SoilState.Untilled) return false;
            UpdateSoilState(SoilState.Tilled);
            return true;
        }

        public bool TryWater()
        {
            if (Soil != SoilState.Tilled) return false;
            UpdateSoilState(SoilState.Watered);
            return true;
        }

        public bool TryPlant(string cropId)
        {
            if (Soil != SoilState.Tilled || Crop != GrowthStage.Empty) return false;
            UpdateCropState(GrowthStage.Seed, cropId);
            return true;
        }

        public bool TryHarvest()
        {
            if (Crop != GrowthStage.Mature) return false;
            UpdateCropState(GrowthStage.Empty, string.Empty);
            return true;
        }

        public bool ProcessDailyGrowth()
        {
            if (Crop == GrowthStage.Empty || Soil != SoilState.Watered) return false;

            if (Crop == GrowthStage.Seed) { UpdateCropState(GrowthStage.Growing, CurrentCropId); return true; }
            if (Crop == GrowthStage.Growing) { UpdateCropState(GrowthStage.Mature, CurrentCropId); return true; }
            
            return false;
        }

        public bool ProcessDailyDrying()
        {
            if (Soil == SoilState.Watered) { UpdateSoilState(SoilState.Tilled); return true; }
            if (Soil == SoilState.Tilled && Crop == GrowthStage.Empty) { UpdateSoilState(SoilState.Untilled); return true; }
            return false;
        }
    }
}
using System;

using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;

namespace GinRummy.Client.ViewModels
{
    public class HouseRulesViewModel : ViewModelBase
    {
        public HouseRulesViewModel(SampleDataService dataService)
        {
            if (dataService == null)
            {
                throw new ArgumentNullException(nameof(dataService));
            }

            GameRules rules = dataService.GetGameRules();
            CardsPerHand = rules.CardsPerHand;
            KnockThreshold = rules.KnockThreshold;
            GinBonus = rules.GinBonus;
            UndercutBonus = rules.UndercutBonus;
            StockCardsToVoid = rules.StockCardsToVoid;
            TargetScore = rules.TargetScore;
        }

        public int CardsPerHand { get; private set; }
        public int KnockThreshold { get; private set; }
        public int GinBonus { get; private set; }
        public int UndercutBonus { get; private set; }
        public int StockCardsToVoid { get; private set; }
        public int TargetScore { get; private set; }
    }
}

namespace LibrarySystem.ViewModels
{
    public class ItemTransfersViewModel
    {
        public List<ItemTransferIndexViewModel> OutgoingRequests { get; set; } = new();

        public List<ItemTransferIndexViewModel> IncomingTransferRequests { get; set; } = new();

        public List<ItemTransferIndexViewModel> HistoricalTransfers { get; set; } = new();
    }
}
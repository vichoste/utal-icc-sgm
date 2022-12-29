namespace Utal.Icc.Sgm.Views.Shared;

public class ErrorViewModel {
	public string? RequestId { get; set; }
	public bool ShowRequestId => !string.IsNullOrEmpty(this.RequestId);
}
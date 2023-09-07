using ReactiveUI;

namespace AvaloniaChatGPT.ViewModels;

public class ViewModelBase : ReactiveObject
{
    public bool IsViewVisible { get; set; }
}

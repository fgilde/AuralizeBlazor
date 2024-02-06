using AuralizeBlazor.Options;
using MudBlazor;
using MudBlazor.Extensions.Core;
using MudBlazor.Utilities;

namespace AuralizeBlazor.Sample.Layout;

public partial class MainLayout
{
    private static readonly MudExColor[] defaultPalette = ["#0bc", "#2cb", "#0bc", "#09c", "#36b", "#2cb"];
    private MudExColor[] _palette = defaultPalette;
    private MudTheme _currentTheme = CreateDefaultTheme();

    private static MudTheme CreateDefaultTheme() =>
        new()
        {
            PaletteDark = new PaletteDark
            {
                Primary = (defaultPalette.First().ToMudColor()),
                Secondary = (defaultPalette.Last().ToMudColor())
            }
        };

    bool _drawerOpen;
    internal static MainLayout Instance;

    void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }

    protected override void OnInitialized()
    {
        Instance = this;
        base.OnInitialized();
    }

    internal Task ColorsChanged(AudioMotionGradient arg)
    {
        if (arg.ColorStops is not { Length: > 0 })
        {
            _palette = defaultPalette;
            _currentTheme = CreateDefaultTheme();
            return Task.CompletedTask;
        }

        _palette =  arg?.ColorStops?.Select(x => new MudExColor(x.Color))?.ToArray();
        _currentTheme = new()
        {
            PaletteDark = new PaletteDark
            {
                Primary = new MudColor(arg.ColorStops.First().Color),
                Secondary = new MudColor(arg.ColorStops.Last().Color),
            }
        };
        return Task.CompletedTask;
    }
}
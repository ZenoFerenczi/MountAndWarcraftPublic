using SandBox.View.Map;
using TaleWorlds.Engine.GauntletUI;

namespace MountAndWarcraftReborn.Magic.UI
{
    public class MWRMagicMapView : MapView
    {
        private MWRMagicMapVM? _vm;
        private GauntletLayer? _layer;
        private GauntletMovieIdentifier? _movie;

        protected override void CreateLayout()
        {
            base.CreateLayout();
            _vm = new MWRMagicMapVM();
            _layer = new GauntletLayer("GauntletLayer", 100);
            _movie = _layer.LoadMovie("MWRMagicMapWidget", _vm);
            MapScreen.AddLayer(_layer);
        }

        protected override void OnMapScreenUpdate(float dt)
        {
            base.OnMapScreenUpdate(dt);
            _vm?.RefreshValues();
        }

        protected override void OnFinalize()
        {
            _vm = null;

            if (_layer != null && _movie != null)
            {
                _layer.ReleaseMovie(_movie);
                MapScreen.RemoveLayer(_layer);
            }

            _movie = null;
            _layer = null;
            base.OnFinalize();
        }
    }
}

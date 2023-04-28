using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using System;

namespace Figurator.Models.Shapes {
    public static class GeometryDop {
        public static string Stringify(this Geometry geom) {
            if (geom is not GeometricalShape @shake) throw new Exception("Geometry не является предком GeometryShake");
            return @shake.saved_path;
        }
        public static Geometry MyParse(this Geometry _, string s) => GeometricalShape.Parse(s);
    }

    public class GeometricalShape: Geometry {

        IStreamGeometryImpl? _impl;
        public string saved_path;

        public GeometricalShape(string path = "") { saved_path = path; }

        private GeometricalShape(string path, IStreamGeometryImpl impl) { saved_path = path; _impl = impl; }

        public static new GeometricalShape Parse(string s) {

            var geometryShake = new GeometricalShape(s);

            using (var context = geometryShake.Open())
            using (var parser = new PathMarkupParser(context)) parser.Parse(s);

            return geometryShake;
        }

        public override Geometry Clone() {
            return new GeometricalShape(saved_path, ((IStreamGeometryImpl) PlatformImpl).Clone());
        }

        public StreamGeometryContext Open() {
            return new StreamGeometryContext(((IStreamGeometryImpl) PlatformImpl).Open());
        }

        protected override IGeometryImpl CreateDefiningGeometry() {
            if (_impl == null) {
                var factory = AvaloniaLocator.Current.GetService<IPlatformRenderInterface>() ?? throw new Exception("Factory not found");
                _impl = factory.CreateStreamGeometry();
            }
            return _impl;
        }
    }
}

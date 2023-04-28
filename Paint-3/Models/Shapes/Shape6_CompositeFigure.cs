﻿using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using static Figurator.Models.Shapes.PropsN;

namespace Figurator.Models.Shapes {
    public class Shape6_CompositeFigure: IShape {
        private static readonly PropsN[] props = new[] { PName, PCommands, PColor, PThickness, PFillColor };

        /*
         * IShape-часть:
         */

        public PropsN[] Props => props;

        public string Name => "Композитка";

        public Shape? Build(Mapper map) {
            if (map.GetProp(PName) is not string @name) return null;

            if (map.GetProp(PCommands) is not SafeGeometry @commands || !@commands.Valid) return null;

            if (map.GetProp(PColor) is not string @color) return null;

            if (map.GetProp(PFillColor) is not string @fillColor) return null;

            if (map.GetProp(PThickness) is not int @thickness) return null;

            return new Path {
                Name = "sn_" + @name,
                Data = @commands.Geometry,
                Stroke = new SolidColorBrush(Color.Parse(@color)),
                Fill = new SolidColorBrush(Color.Parse(@fillColor)),
                StrokeThickness = @thickness
            };
        }
        public bool Load(Mapper map, Shape shape) {
            if (shape is not Path @path) return false;
            if (@path.Stroke == null || @path.Fill == null) return false;

            if (map.GetProp(PCommands) is not SafeGeometry @commands) return false;

            @commands.Set(@path.Data.Stringify());

            map.SetProp(PColor, ((SolidColorBrush) @path.Stroke).Color.ToString());
            map.SetProp(PFillColor, ((SolidColorBrush) @path.Fill).Color.ToString());
            map.SetProp(PThickness, (int) @path.StrokeThickness);

            return true;
        }



        public Dictionary<string, object?>? Export(Shape shape) {
            if (shape is not Path @path) return null;
            if (@path.Name == null || !@path.Name.StartsWith("sn_")) return null;

            return new() {
                ["name"] = @path.Name[3..],
                ["path"] = @path.Data.Stringify(),
                ["stroke"] = @path.Stroke,
                ["fill"] = @path.Fill,
                ["thickness"] = (int) @path.StrokeThickness
            };
        }
        public Shape? Import(Dictionary<string, object?> data) {
            if (!data.ContainsKey("name") || data["name"] is not string @name) return null;

            if (!data.ContainsKey("path") || data["path"] is not string @path) return null;
            var commands = new SafeGeometry(@path);

            if (!data.ContainsKey("stroke") || data["stroke"] is not SolidColorBrush @color) return null;
            if (!data.ContainsKey("fill") || data["fill"] is not SolidColorBrush @fillColor) return null;
            if (!data.ContainsKey("thickness") || data["thickness"] is not short @thickness) return null;

            return new Path {
                Name = "sn_" + @name,
                Data = commands.Geometry,
                Stroke = @color,
                Fill = @fillColor,
                StrokeThickness = @thickness
            };
        }



        public Point? GetPos(Shape shape) { // Центр между всеми M x y
            if (shape is not Path @path) return null;

            var geom = @path.Data.Stringify().NormSplit();
            int x = 0, y = 0, c = 0;
            for (int i = 0; i < geom.Length; i++)
                if (geom[i] == "M" && i + 2 < geom.Length && int.TryParse(geom[i + 1], out int @X) && int.TryParse(geom[i + 2], out int @Y)) {
                    x += @X;
                    y += @Y;
                    c += 1;
                }
            return c == 0 ? new Point() : new Point(x / c, y / c);
        }
        public bool SetPos(Shape shape, int x, int y) {
            var old = GetPos(shape);
            if (old == null) return false;

            var path = (Path) shape;
            Point delta = new Point(x, y) - (Point) old;

            var geom = path.Data.Stringify().NormSplit();
            for (int i = 0; i < geom.Length; i++)
                if (geom[i] == "M" && i + 2 < geom.Length && int.TryParse(geom[i + 1], out int @X) && int.TryParse(geom[i + 2], out int @Y)) {
                    geom[i + 1] = (@X + delta.X).ToString();
                    geom[i + 2] = (@Y + delta.Y).ToString();
                } // ЮХУ! С первого раза идея проканала! ;'-}

            var geom_s = string.Join(' ', geom);
            if (geom.Length > 0 && geom[0] != "M") geom_s = "M " + delta.X + " " + delta.Y + " " + geom_s;

            //name[2] = Utils.Base64Encode(geom_s);
            //path.Name = string.Join('|', name);
            // Name нельзя редактировать :/// Вот так вот и появился GeometryShake класс, т.к. это уже достало))) Формально, только из-за УДАЧНОГО добавления Stringify в Geometry
            // Возможно, это слегка напоминает плохое программирование, но всяко лучше крашущийся программы из-за readonly path.Name

            var commands = new SafeGeometry(geom_s);
            path.Data = commands.Geometry;

            return true;
        }
    }
}

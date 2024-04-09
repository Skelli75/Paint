using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Ink;


namespace Paint
{
    internal class StateHandler
    {
        List<StrokeCollection> _added; // två listor, en med sträck som finns och en med borttagna
        List<StrokeCollection> _visualStrokes; // två listor, en med sträck som finns och en med borttagna
        List<StrokeCollection> _removed;
        InkCanvas _canvas;

        public StateHandler(InkCanvas canvas) 
        {
            _added = new();
            _visualStrokes = new();
            _removed = new();

            _canvas = canvas;
        }

        public void AddToCanvas(StrokeCollection stroke)
        {
            _canvas.Strokes.Add(stroke);
        }

        public void RemoveVisualLine() 
        {
            if (_added.Count > 0 && _visualStrokes.Count > 0)
            {
                if (_added[_added.Count - 1] != _visualStrokes[_visualStrokes.Count - 1]) // är det senaste sträcket ett permanent sträck?
                {
                    _canvas.Strokes.Remove(_visualStrokes[_visualStrokes.Count - 1]); // ta då bort det
                }
            }
        }

        public void AddToAdded(StrokeCollection stroke) 
        {
            _added.Add(stroke);
        }

        public void AddToVisualStrokes(StrokeCollection stroke)
        {
            _visualStrokes.Add(stroke);
        }

        public void AddVisualLineToCanvas()
        {
            _added.Add(new StrokeCollection() { _visualStrokes[_visualStrokes.Count - 1] });
            _visualStrokes.Clear(); // tömmer listan
        }

        public void RemoveStroke(StrokeCollection stroke) 
        { 
            
        }

        public void ClearAdded()
        {
            _added.Clear();
        }

        public void ClearRemove() 
        {
            _removed.Clear();
        }

        public int GetLength()
        {
            return _added.Count;
        }

        public void Undo()
        {
            if (_added.Count > 1)
            {
                _removed.Add(_added[_added.Count - 1]); // sparar det senaste ritade sträcket för att senare kunna redo:a om det önskas
                _canvas.Strokes.Remove(_added[_added.Count - 1]); // tar bort det senaste ritade sträcket från tavlan
                _added.Remove(_added[_added.Count - 1]); // tar bort sträcket från "finns på tavlan" listan
            }
        }

        public void Redo()
        {
            if (_added.Count >= 0 && _removed.Count > 0)
            {
                _added.Add(_removed[_removed.Count - 1]); // lägger tillbaka det senaste borttagna sträcket i "finns på tavlan" listan
                _canvas.Strokes.Add(_removed[_removed.Count - 1]); // lägger tillbaka sträcket på tavlan
                _removed.Remove(_removed[_removed.Count - 1]); // tar bort sträcket från listan med borttagna sträck
            }
        }
    }
}

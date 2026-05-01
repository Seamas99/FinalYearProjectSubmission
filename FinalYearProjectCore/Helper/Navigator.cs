using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FinalYearProjectCore.Helper
{
    public class Navigator<T>
    {
        private readonly IList<T> _items;
        private int _index = -1;
        public int Index
        {
            get { return _index; }
        }

        public Navigator(IList<T> items)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
        }

        //curent position/index in list
        public T Current =>
            _index >= 0 && _index < _items.Count ? _items[_index] : default;

        public bool MoveNext()
        {
            if (_index + 1 >= _items.Count)
                return false;

            _index++;
            return true;
        }

        public bool MovePrevious()
        {
            if (_index - 1 < 0)
                return false;

            _index--;
            return true;
        }

        public bool DeleteCurrent()
        {
            if (_index < 0 || _index >= _items.Count)
                return false;

            _items.RemoveAt(_index);

            // after removal will point to the next item automatically
            // but if the last item in index was remove will go back
            if (_index >= _items.Count)
                _index = _items.Count - 1;

            return true;
        }

    }
}

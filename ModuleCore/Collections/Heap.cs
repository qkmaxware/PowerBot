using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleCore.Collections
{
    public class Heap<T> {

        private static int initialCapacity = 10;
        private static int growthFactor = 2;

        private Comparer<T> comparer;
        private T[] values = new T[initialCapacity];
        private int size = 0;

        public Heap(Comparer<T> comparer) {
            this.comparer = comparer;
        }

        private void BubbleUp(int i) {
            for (; i > 0 && comparer.Compare(values[i], values[i / 2]) < 0; i = i / 2) {
                Swap(i, i / 2);
            }
        }

        private void BubbleDown(int i) {
            int dominantNode = i;

            if (comparer.Compare(values[i], values[i * 2]) > 0)
                dominantNode = i * 2;
            if (comparer.Compare(values[i], values[i * 2 + 1]) > 0)
                dominantNode = i * 2 + 1;

            if (dominantNode == i)
                return;

            Swap(i, dominantNode);
            BubbleDown(dominantNode);
        }

        public void Expand() {
            T[] n = new T[this.values.Length * growthFactor];
            Array.Copy(this.values, n, values.Length);
            this.values = n;
        }

        private void Swap(int i, int j){
            T tmp = values[i];
            values[i] = values[j];
            values[j] = tmp;
        }

        public void Insert(T value) {
            if (size == values.Length - 1)
                Expand();

            int pos = ++size;

            values[pos] = value;
            BubbleUp(pos);
        }

        public T PopFirst() {
            T value = First;

            Swap(0, size - 1); //Swap head and tail
            BubbleDown(0);

            size--;
            return value;
        }

        public T First {
            get {
                return this.values[0];
            }
        }

        public T Tail {
            get {
                return this.values[size - 1];
            }
        }

        public int Count {
            get {
                return size;
            }
        }

        public int Capacity {
            get {
                return this.values.Length;
            }
        }

        public bool IsEmpty {
            get {
                return size == 0;
            }
        }

    }
}

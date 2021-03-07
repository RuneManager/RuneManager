using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuneOptim {

    public class OnSetEventArgs<T> {
        public int i = -1;
        public T val = default(T);
    }

    public abstract class ListProp<T>
        : IList<T> {
        int maxind = -1;

        virtual protected int maxInd {
            get {
                if (maxind == -1) {
                    var type = this.GetType();
                    maxind = type.GetFields().Where(p => Attribute.IsDefined(p, typeof(ListPropertyAttribute)))
                        .Max(p => ((ListPropertyAttribute)p.GetCustomAttributes(typeof(ListPropertyAttribute), false).First()).Index) + 1;

                }
                return maxind;
            }
        }

        public bool PreventOnChange { get; set; }

        public int MaxInd => maxInd;

        virtual protected void OnChange(int i, T val) { }

        private void onChanged(int i, T v) {
            OnChange(i, v);
            OnChanged?.Invoke(this, new OnSetEventArgs<T>() { i = i, val = v });
        }

        public event EventHandler<OnSetEventArgs<T>> OnChanged;

        public int Count {
            get {
                for (int i = 0; i < maxInd; i++) {
                    if (this[i].Equals(default(T)))
                        return i;
                }
                return maxInd;
            }
        }

        virtual public bool IsReadOnly {
            get {
                foreach (var p in Props) {
                    if (this[p.Key].Equals(default(T)))
                        return false;
                }
                return true;
            }
        }

        Dictionary<int, System.Reflection.FieldInfo> props = null;

        Dictionary<int, System.Reflection.FieldInfo> Props {
            get {
                if (props == null) {
                    var type = this.GetType();
                    var pros = type.GetFields().Where(p => Attribute.IsDefined(p, typeof(ListPropertyAttribute)));
                    props = pros.ToDictionary(p => ((ListPropertyAttribute)p.GetCustomAttributes(typeof(ListPropertyAttribute), false).First()).Index);
                }
                return props;
            }
        }

#pragma warning disable S112 // General exceptions should never be thrown
        virtual public T this[int index] {
            get {
                if (Props[index] == null)
                    throw new IndexOutOfRangeException("No class member assigned to that index!");
                return (T)props[index].GetValue(this);
            }
            set {
                if (Props[index] == null)
                    throw new IndexOutOfRangeException("No class member assigned to that index!");

                props[index].SetValue(this, value);
                if (!PreventOnChange)
                    onChanged(index, value);
            }
        }
#pragma warning restore S112 // General exceptions should never be thrown

        public int IndexOf(T item) { throw new NotImplementedException(); }

        public void Insert(int index, T item) { throw new NotImplementedException(); }

        public void RemoveAt(int index) { throw new NotImplementedException(); }

        public void Clear() { throw new NotImplementedException(); }

        public bool Contains(T item) { throw new NotImplementedException(); }

        public void CopyTo(T[] array, int arrayIndex) { throw new NotImplementedException(); }

        public bool Remove(T item) { throw new NotImplementedException(); }

        public IEnumerator<T> GetEnumerator() { return (IEnumerator<T>)new ListPropertyEnumerator<T>(this); }

        IEnumerator IEnumerable.GetEnumerator() { return new ListPropertyEnumerator<T>(this); }

        virtual public void Add(T item) {
            this[Count] = item;
        }
    }

    public class ListPropertyEnumerator<T> : IEnumerator {
        int i = -1;
        readonly ListProp<T> parent;

        public ListPropertyEnumerator(ListProp<T> p) {
            parent = p;
        }

        public object Current {
            get {
                return parent[i];
            }
        }

        public bool MoveNext() {
            i++;
            return (i < parent.MaxInd);
        }

        public void Reset() {
            i = -1;
        }
    }

    public class ListPropertyAttribute : Attribute {
        private int index;

        public int Index { get { return index; } }

        public ListPropertyAttribute(int ind) {
            index = ind;
        }
    }
}

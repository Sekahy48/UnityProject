using System;
using System.Collections.Generic;

namespace Core.ECS.Component
{
    /// <summary>
    /// Declaracion de los campos numericos de un componente: por cada campo, el nombre con
    /// el que viaja en el catalogo y como leerlo y escribirlo.
    ///
    /// Existe para que ese nombre se escriba una sola vez. Antes aparecia dos veces por
    /// campo —una en <see cref="IJsonLoadable.SetFromValues"/> para escribirlo al cargar y
    /// otra en <see cref="INumericFields.TryGetNumericValue"/> para leerlo despues—, y nada
    /// obligaba a que las dos listas coincidieran: bastaba anadir un campo y olvidarse de
    /// una para que una magnitud dejara de resolverse sin que nada avisara. Ahora las dos
    /// direcciones salen de la misma declaracion.
    ///
    /// <para><b>Es estatico por clase, no por instancia.</b> Por eso los accesores reciben
    /// el componente en vez de capturarlo: un mapa por instancia significaria un diccionario
    /// de delegados por cada manzana del mundo, y de los componentes hay tantos como
    /// entidades.</para>
    ///
    /// <para><b>Es una lista y no un diccionario.</b> El orden de declaracion es el orden en
    /// que se aplican los valores, y eso importa: <c>SetHunger</c> recorta contra
    /// <c>maxHunger</c>, asi que aplicar el hambre antes que su maximo la recortaria contra
    /// cero. La busqueda por nombre recorre la lista, que nunca pasa de una docena de
    /// entradas y se consulta en momentos puntuales, no por fotograma.</para>
    /// </summary>
    /// <typeparam name="T">Componente al que pertenecen los campos</typeparam>
    public class NumericFields<T> where T : class
    {
        private class Field
        {
            public string Name;
            public Func<T, float> Get;
            public Action<T, float> Set;
        }

        private readonly List<Field> _fields = new List<Field>();

        /// <summary>
        /// Declara un campo. Devuelve el propio mapa para poder encadenar.
        ///
        /// El escritor se declara aparte del lector, y no como acceso directo al campo,
        /// porque muchos componentes recortan o validan al escribir —la durabilidad se
        /// limita a su maximo, el hambre a su tope—. Pasando por el metodo que ya existe,
        /// cargar desde el catalogo respeta las mismas reglas que cualquier otra escritura.
        /// </summary>
        /// <param name="name">Nombre del campo tal como aparece en el catalogo</param>
        /// <param name="get">Como leerlo</param>
        /// <param name="set">Como escribirlo. Los campos enteros reciben aqui el float y lo
        /// truncan: el catalogo no deberia traer decimales en un entero, y si los trae, se
        /// pierden igual que se perderian al asignarlos.</param>
        public NumericFields<T> Add(string name, Func<T, float> get, Action<T, float> set)
        {
            _fields.Add(new Field { Name = name, Get = get, Set = set });
            return this;
        }

        /// <summary>
        /// Lee un campo por nombre.
        /// </summary>
        /// <returns>True si el campo esta declarado</returns>
        public bool TryGet(T owner, string name, out float value)
        {
            foreach (Field field in _fields)
            {
                if (field.Name == name)
                {
                    value = field.Get(owner);
                    return true;
                }
            }

            value = 0f;
            return false;
        }

        /// <summary>
        /// Escribe los campos declarados que vengan en los valores del catalogo, en orden de
        /// declaracion. Lo que no venga se queda como estaba.
        /// </summary>
        public void Apply(T owner, Dictionary<string, object> values)
        {
            if (values == null) return;

            foreach (Field field in _fields)
            {
                if (values.TryGetValue(field.Name, out object raw) && raw != null)
                {
                    field.Set(owner, Convert.ToSingle(raw));
                }
            }
        }

        /// <summary>Nombres declarados, en orden.</summary>
        public IEnumerable<string> Names
        {
            get
            {
                foreach (Field field in _fields) yield return field.Name;
            }
        }
    }
}

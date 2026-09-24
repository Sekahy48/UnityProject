using System;
using System.Collections.Generic;
using System.Globalization;

namespace Core.ECS.Component.ItemComponents
{
    /// <summary>
    /// Que modelo 3D representa a este item, y de que depende.
    ///
    /// Guarda rutas relativas a la carpeta de datos, no mallas: <c>Core</c> no sabe cargar un
    /// fichero glTF ni tiene por que. Traducir una ruta en algo dibujable es trabajo de la
    /// capa de Unity; aqui solo vive la regla de cual de las rutas toca.
    ///
    /// Un item con un solo modelo tiene una etapa de umbral 0 y sin magnitud. No hay camino
    /// especial para ese caso: es el caso general con una etapa.
    /// </summary>
    public class ModelComponent : IComponent, IJsonLoadable
    {
        private string _drivenBy;
        private List<ModelStage> _stages = new List<ModelStage>();

        public ModelComponent() {}

        public ModelComponent(string drivenBy, List<ModelStage> stages)
        {
            _drivenBy = drivenBy;
            _stages = stages ?? new List<ModelStage>();
        }

        /// <summary>
        /// Magnitud que decide la etapa, cualificada como <c>Componente.campo</c>. Es null
        /// cuando hay una sola etapa, porque entonces no hay nada que decidir.
        /// </summary>
        public string DrivenBy => _drivenBy;

        /// <summary>Etapas de mayor a menor umbral.</summary>
        public IReadOnlyList<ModelStage> Stages => _stages;

        public bool HasStages => _stages.Count > 0;

        /// <summary>
        /// Etapa que se usa cuando no hay magnitud, no se puede leer, o su valor no alcanza
        /// ningun umbral: la de umbral mas bajo.
        /// </summary>
        public ModelStage DefaultStage => _stages.Count == 0 ? null : _stages[_stages.Count - 1];

        /// <summary>
        /// Etapa que corresponde a un valor de la magnitud.
        ///
        /// Recorre de mayor a menor y se queda con la primera que el valor alcanza. Si no
        /// alcanza ninguna devuelve la mas baja en vez de null: un item mal configurado debe
        /// verse feo, no verse invisible, y un hueco en el mundo es mucho mas dificil de
        /// diagnosticar que un modelo que no cambia.
        /// </summary>
        public ModelStage StageFor(float value)
        {
            foreach (ModelStage stage in _stages)
            {
                if (value >= stage.Threshold) return stage;
            }

            return DefaultStage;
        }

        /// <summary>
        /// Lee la forma en que Stack&amp;Go serializa las etapas:
        /// <c>"0.7=models/a.glb,models/b.glb;0=models/c.glb"</c>.
        ///
        /// Se reordena despues de leer aunque el exportador ya las mande ordenadas. El orden
        /// es de lo que depende <see cref="StageFor"/>, y confiar en que el otro lado lo
        /// mantenga significa que el dia que cambie alli, aqui falla en silencio eligiendo
        /// siempre la etapa mas baja.
        /// </summary>
        public void SetFromValues(Dictionary<string, object> values)
        {
            if (values.ContainsKey("drivenBy")) _drivenBy = Convert.ToString(values["drivenBy"]);

            _stages = new List<ModelStage>();

            if (!values.ContainsKey("stages")) return;

            string raw = Convert.ToString(values["stages"]);
            if (string.IsNullOrWhiteSpace(raw)) return;

            foreach (string chunk in raw.Split(';'))
            {
                ModelStage stage = ParseStage(chunk);
                if (stage != null) _stages.Add(stage);
            }

            _stages.Sort((a, b) => b.Threshold.CompareTo(a.Threshold));
        }

        /// <summary>
        /// Lee un tramo <c>umbral=ruta,ruta</c>.
        ///
        /// El umbral se interpreta siempre con cultura invariante. Con la cultura del sistema,
        /// un Windows en espanol leeria <c>"0.7"</c> como el numero siete: el item no fallaria
        /// al cargar, simplemente no alcanzaria nunca esa etapa.
        /// </summary>
        /// <returns>La etapa, o null si el tramo esta mal formado o no tiene rutas</returns>
        private ModelStage ParseStage(string chunk)
        {
            if (string.IsNullOrWhiteSpace(chunk)) return null;

            int separator = chunk.IndexOf('=');
            if (separator <= 0) return null;

            if (!float.TryParse(chunk.Substring(0, separator), NumberStyles.Float, CultureInfo.InvariantCulture, out float threshold))
                return null;

            List<string> paths = new List<string>();

            foreach (string path in chunk.Substring(separator + 1).Split(','))
            {
                string trimmed = path.Trim();
                if (trimmed.Length > 0) paths.Add(trimmed);
            }

            return paths.Count == 0 ? null : new ModelStage(threshold, paths);
        }

        /// <summary>
        /// Las etapas se comparten con el clon en vez de copiarse: son inmutables, asi que no
        /// hay forma de que un clon le cambie el aspecto a otro.
        /// </summary>
        public IComponent Clone()
        {
            return new ModelComponent(_drivenBy, _stages);
        }

        public bool Equivalent(IComponent other)
        {
            if (!(other is ModelComponent otherModel)) return false;
            if (_drivenBy != otherModel._drivenBy) return false;
            if (_stages.Count != otherModel._stages.Count) return false;

            for (int i = 0; i < _stages.Count; i++)
            {
                if (!_stages[i].Equivalent(otherModel._stages[i])) return false;
            }

            return true;
        }
    }
}

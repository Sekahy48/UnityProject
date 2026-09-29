using System;
using System.Collections.Generic;
using Core.MVC.View.UI.Radial;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVC.View.Controls
{
    /// <summary>
    /// Menu radial reutilizable de UI Toolkit: anillos de quesitos alrededor del centro de la
    /// pantalla, elegidos por la direccion y la distancia del puntero.
    ///
    /// <para><b>No sabe de que es el menu.</b> Recibe textos y avisa de posiciones
    /// (anillo, indice). Que una opcion tenga hijos lo sabe quien lo usa: al recibir el clic,
    /// pide un anillo nuevo con <see cref="AddRing"/>. El control solo dibuja y mide.</para>
    ///
    /// <para><b>Anillos.</b> El primero es la rueda completa. Cada anillo siguiente se abre
    /// por fuera del anterior, en un arco centrado en la opcion padre: sus porciones miden
    /// como mucho lo que mide la del padre, o <c>--radial-child-sector</c> grados si es
    /// menor. Que hay bajo el puntero se decide con la distancia al centro (que anillo) y el
    /// angulo (que porcion); la cuenta es la misma que la del dibujo.</para>
    ///
    /// <para><b>Los quesitos se dibujan con Painter2D</b> (UI Toolkit no tiene esa forma).
    /// Para que el tema los siga vistiendo, sus colores y medidas se leen de propiedades USS
    /// propias (<c>--radial-fill</c>, <c>--radial-outer-radius</c>...), con valores por
    /// defecto aqui. Los textos si son elementos normales, con clases USS.</para>
    ///
    /// <para>Mientras esta abierto, la capa ocupa toda la pantalla y SI captura el puntero:
    /// necesita los movimientos para resaltar y los clics para elegir o cerrar.</para>
    /// </summary>
    public class RadialMenu : VisualElement
    {
        #region Estilo (propiedades USS propias)

        private static readonly CustomStyleProperty<Color> FILL = new CustomStyleProperty<Color>("--radial-fill");
        private static readonly CustomStyleProperty<Color> FILL_HIGHLIGHT = new CustomStyleProperty<Color>("--radial-fill-highlight");
        private static readonly CustomStyleProperty<Color> STROKE = new CustomStyleProperty<Color>("--radial-stroke");
        private static readonly CustomStyleProperty<Color> STROKE_HIGHLIGHT = new CustomStyleProperty<Color>("--radial-stroke-highlight");
        private static readonly CustomStyleProperty<float> STROKE_WIDTH = new CustomStyleProperty<float>("--radial-stroke-width");
        private static readonly CustomStyleProperty<float> INNER_RADIUS = new CustomStyleProperty<float>("--radial-inner-radius");
        private static readonly CustomStyleProperty<float> OUTER_RADIUS = new CustomStyleProperty<float>("--radial-outer-radius");
        private static readonly CustomStyleProperty<float> GAP_DEGREES = new CustomStyleProperty<float>("--radial-gap");
        private static readonly CustomStyleProperty<float> RING_WIDTH = new CustomStyleProperty<float>("--radial-ring-width");
        private static readonly CustomStyleProperty<float> RING_GAP = new CustomStyleProperty<float>("--radial-ring-gap");
        private static readonly CustomStyleProperty<float> CHILD_SECTOR = new CustomStyleProperty<float>("--radial-child-sector");

        private Color _fill = new Color(0.12f, 0.12f, 0.12f, 0.88f);
        private Color _fillHighlight = new Color(0.3f, 0.3f, 0.3f, 0.95f);
        private Color _stroke = new Color(0.6f, 0.6f, 0.6f, 0.9f);
        private Color _strokeHighlight = Color.white;
        private float _strokeWidth = 2f;
        private float _inner = 48f;
        private float _outer = 150f;
        private float _gapDegrees = 2f;
        private float _ringWidth = 90f;        /* grosor de los anillos 2, 3... */
        private float _ringGap = 8f;           /* separacion entre un anillo y el siguiente */
        private float _childSectorDegrees = 50f;

        #endregion

        /// <summary>
        /// Mas alla del borde exterior del PRIMER anillo se sigue eligiendo un poco (un gesto
        /// rapido se pasa); a partir de aqui es "fuera": no resalta y un clic cierra.
        /// </summary>
        private const float OUTSIDE_FACTOR = 1.6f;

        /// <summary>Holgura, en pixeles, por fuera del ultimo anillo exterior.</summary>
        private const float OUTER_RING_TOLERANCE = 40f;

        private const float TWO_PI = Mathf.PI * 2f;

        /// <summary>El puntero entro en (anillo, indice), o en nada (-1, -1).</summary>
        public event Action<int, int> OnHighlighted;
        /// <summary>Clic sobre (anillo, indice).</summary>
        public event Action<int, int> OnClicked;
        public event Action OnDismissed;

        /// <summary>
        /// Un anillo: sus textos y su geometria. La geometria se recalcula entera en
        /// <see cref="Relayout"/>, porque el arco de un anillo depende del de su padre y los
        /// radios pueden cambiar cuando el tema se resuelve.
        /// </summary>
        private sealed class Ring
        {
            public readonly List<Label> Labels = new List<Label>();
            public int Parent;      /* indice de la opcion padre en el anillo anterior; -1 en el primero */
            public float Inner;     /* radio interior */
            public float Outer;     /* radio exterior */
            public float Start;     /* angulo del borde inicial: 0 arriba, horario */
            public float Sector;    /* ancho angular de cada porcion */

            public int Count => Labels.Count;
            public float CenterOf(int i) => Start + Sector * (i + 0.5f);
        }

        private readonly VisualElement _wheel;
        private readonly VisualElement _hub;
        private readonly List<Ring> _rings = new List<Ring>();
        private int _hiLevel = -1;
        private int _hiIndex = -1;

        public bool IsOpen { get; private set; }

        /// <summary>
        /// Ruta en Resources de la hoja propia del control
        /// (Assets/UI Toolkit/Controls/Resources/Controls/RadialMenu.uss).
        /// </summary>
        private const string STYLE_SHEET = "Controls/RadialMenu";

        /// <summary>Una carga para todos los radiales: la hoja es la misma.</summary>
        private static StyleSheet _sheet;

        public RadialMenu()
        {
            AddToClassList("radial-root");
            pickingMode = PickingMode.Ignore;
            style.display = DisplayStyle.None;

            // La rueda es un punto sin tamano en el centro de la pantalla; todo se coloca y
            // se dibuja respecto a el. Dibujar fuera de los limites del elemento es legal en
            // UI Toolkit, y asi el centro no depende de ningun tamano.
            _wheel = new VisualElement();
            _wheel.AddToClassList("radial-ring");
            _wheel.pickingMode = PickingMode.Ignore;
            _wheel.generateVisualContent += DrawSectors;
            Add(_wheel);

            _hub = new VisualElement();
            _hub.AddToClassList("radial-hub");
            _hub.pickingMode = PickingMode.Ignore;
            _wheel.Add(_hub);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<CustomStyleResolvedEvent>(OnStyleResolved);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
        }

        /// <summary>
        /// El control trae su estructura consigo: al entrar en un panel engancha su hoja en
        /// la raiz del panel si aun no esta. Asi quien crea un RadialMenu no tiene que saber
        /// que hoja cargar, y el radial funciona igual en cualquier documento.
        ///
        /// <para>En la raiz y no en el propio control a proposito: es la posicion mas lejana
        /// de los elementos, asi que a igual especificidad las hojas de los documentos
        /// —el tema— le ganan. En el propio control seria al reves y el tema no podria
        /// cambiar nada que la base defina.</para>
        /// </summary>
        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (_sheet == null) _sheet = Resources.Load<StyleSheet>(STYLE_SHEET);
            if (_sheet == null)
            {
                Debug.LogError($"RadialMenu: no se encuentra la hoja 'Resources/{STYLE_SHEET}.uss'.");
                return;
            }

            VisualElement root = evt.destinationPanel.visualTree;
            if (!root.styleSheets.Contains(_sheet)) root.styleSheets.Add(_sheet);
        }

        #region Abrir, anadir anillos y cerrar

        /// <summary>
        /// Texto del aviso bajo una opcion con subopciones. Vive aqui (vista) y no en el
        /// presentador: el presentador dice QUE opciones tienen hijos; como se avisa lo decide
        /// la vista, y el tema puede restilarlo u ocultarlo (<c>.radial-option__more</c>).
        /// </summary>
        private const string MORE_OPTIONS_HINT = "[M\u00e1s opciones]";

        /// <summary>Abre el menu con un solo anillo: la rueda completa.</summary>
        /// <param name="hasChildren">Paralela a <paramref name="labels"/>: true si esa opcion
        /// abre un anillo. Null o mas corta: ninguna (o las que falten) sin hijos.</param>
        public void Open(IReadOnlyList<string> labels, IReadOnlyList<bool> hasChildren = null)
        {
            RemoveRingsFrom(0);
            AppendRing(-1, labels, hasChildren);

            SetHighlighted(-1, -1, notify: false);
            IsOpen = true;
            pickingMode = PickingMode.Position;
            style.display = DisplayStyle.Flex;
            Relayout();
        }

        /// <summary>
        /// Abre un anillo nuevo por fuera del anillo mas exterior, colgando de su opcion
        /// <paramref name="parentIndex"/>. Para sustituir un anillo ya abierto, primero
        /// <see cref="CloseRingsAbove"/> y luego esto.
        /// </summary>
        public void AddRing(int parentIndex, IReadOnlyList<string> labels)
        {
            if (!IsOpen || _rings.Count == 0) return;
            Ring outermost = _rings[_rings.Count - 1];
            if (parentIndex < 0 || parentIndex >= outermost.Count) return;
            if (labels == null || labels.Count == 0) return;

            AppendRing(parentIndex, labels, null);
            Relayout();
        }

        /// <summary>
        /// Quita los anillos por encima de <paramref name="level"/> (0 = el primero, que se
        /// queda). Si lo resaltado estaba en un anillo quitado, deja de estarlo.
        /// </summary>
        public void CloseRingsAbove(int level)
        {
            if (level < 0) level = 0;
            if (_rings.Count <= level + 1) return;

            RemoveRingsFrom(level + 1);
            if (_hiLevel > level) SetHighlighted(-1, -1, notify: true);
            Relayout();
        }

        public void Close()
        {
            IsOpen = false;
            pickingMode = PickingMode.Ignore;
            style.display = DisplayStyle.None;
            SetHighlighted(-1, -1, notify: false);
            RemoveRingsFrom(0);
        }

        private void AppendRing(int parent, IReadOnlyList<string> labels, IReadOnlyList<bool> hasChildren)
        {
            Ring ring = new Ring { Parent = parent };
            bool first = _rings.Count == 0;

            for (int i = 0; i < labels.Count; i++)
            {
                Label label = new Label(labels[i]);
                label.AddToClassList("radial-option");
                if (first && i == 0) label.AddToClassList("radial-option--default");
                if (!first) label.AddToClassList("radial-option--child");
                label.pickingMode = PickingMode.Ignore;

                // Opcion con subopciones: clase para que el tema la distinga y un aviso debajo,
                // para que se sepa antes de elegir que no ejecuta sino que abre mas opciones.
                if (hasChildren != null && i < hasChildren.Count && hasChildren[i])
                {
                    label.AddToClassList("radial-option--branch");
                    Label more = new Label(MORE_OPTIONS_HINT);
                    more.AddToClassList("radial-option__more");
                    more.pickingMode = PickingMode.Ignore;
                    label.Add(more);
                }
                _wheel.Add(label);
                ring.Labels.Add(label);
            }

            _rings.Add(ring);
        }

        private void RemoveRingsFrom(int level)
        {
            for (int k = _rings.Count - 1; k >= level; k--)
            {
                foreach (Label label in _rings[k].Labels) label.RemoveFromHierarchy();
                _rings.RemoveAt(k);
            }
        }

        /// <summary>
        /// Recalcula radios y arcos de todos los anillos y coloca los textos en el centro de
        /// su porcion. El primero cubre la vuelta entera con la opcion 0 arriba (igual que
        /// <see cref="RadialGeometry"/>); cada siguiente, un arco centrado en su padre.
        /// </summary>
        private void Relayout()
        {
            for (int k = 0; k < _rings.Count; k++)
            {
                Ring ring = _rings[k];
                int n = ring.Count;
                if (n == 0) continue;

                if (k == 0)
                {
                    ring.Inner = _inner;
                    ring.Outer = _outer;
                    ring.Sector = TWO_PI / n;
                    ring.Start = -ring.Sector * 0.5f;
                }
                else
                {
                    Ring parent = _rings[k - 1];
                    ring.Inner = parent.Outer + _ringGap;
                    ring.Outer = ring.Inner + _ringWidth;
                    ring.Sector = Mathf.Min(parent.Sector, _childSectorDegrees * Mathf.Deg2Rad);
                    ring.Start = parent.CenterOf(ring.Parent) - ring.Sector * n * 0.5f;
                }

                float mid = (ring.Inner + ring.Outer) * 0.5f;
                for (int i = 0; i < n; i++)
                {
                    float a = ring.CenterOf(i);
                    ring.Labels[i].style.left = Mathf.Sin(a) * mid;
                    ring.Labels[i].style.top = -Mathf.Cos(a) * mid;
                }
            }

            _wheel.MarkDirtyRepaint();
        }

        #endregion

        #region Puntero

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!IsOpen) return;
            (int level, int index) = HitAt(evt.localPosition);
            SetHighlighted(level, index, notify: true);
        }

        /// <summary>
        /// Clic en un quesito: esa posicion. Clic en el centro o fuera: cerrar sin elegir. Se
        /// recalcula con la posicion del clic, por si llega sin un movimiento previo.
        /// </summary>
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!IsOpen) return;

            (int level, int index) = HitAt(evt.localPosition);
            evt.StopPropagation();

            if (index >= 0) OnClicked?.Invoke(level, index);
            else OnDismissed?.Invoke();
        }

        /// <summary>
        /// Que hay bajo el puntero. Primero los anillos exteriores, de fuera hacia dentro:
        /// dentro de su banda de radios Y de su arco. Si ninguno, el primer anillo con su
        /// holgura de siempre (<see cref="OUTSIDE_FACTOR"/>): asi, apuntando en la banda de un
        /// anillo exterior pero fuera de su arco, se sigue eligiendo en el primero.
        /// </summary>
        private (int level, int index) HitAt(Vector3 local)
        {
            if (_rings.Count == 0) return (-1, -1);

            Rect r = contentRect;
            float dx = local.x - r.width * 0.5f;
            float dy = local.y - r.height * 0.5f;
            float distance = Mathf.Sqrt(dx * dx + dy * dy);

            // atan2(x, -y) da 0 arriba y crece en sentido horario con y hacia abajo.
            float angle = Mathf.Atan2(dx, -dy);
            if (angle < 0f) angle += TWO_PI;

            float halfGap = _ringGap * 0.5f;
            for (int k = _rings.Count - 1; k >= 1; k--)
            {
                Ring ring = _rings[k];
                float outerLimit = k == _rings.Count - 1 ? ring.Outer + OUTER_RING_TOLERANCE : ring.Outer + halfGap;
                if (distance < ring.Inner - halfGap || distance > outerLimit) continue;

                int i = IndexInArc(angle, ring);
                if (i >= 0) return (k, i);
            }

            int first = RadialGeometry.IndexAt(dx, dy, _rings[0].Count, _inner, _outer * OUTSIDE_FACTOR);
            return first >= 0 ? (0, first) : (-1, -1);
        }

        /// <summary>Porcion del arco del anillo que contiene el angulo, o -1 si cae fuera.</summary>
        private static int IndexInArc(float angle, Ring ring)
        {
            float rel = angle - ring.Start;
            rel %= TWO_PI;
            if (rel < 0f) rel += TWO_PI;

            if (rel >= ring.Sector * ring.Count) return -1;
            return Mathf.Min((int)(rel / ring.Sector), ring.Count - 1);
        }

        private void SetHighlighted(int level, int index, bool notify)
        {
            if (level == _hiLevel && index == _hiIndex) return;

            _hiLevel = level;
            _hiIndex = index;
            ApplyLabelClasses();

            _hub.EnableInClassList("radial-hub--idle", _hiIndex < 0);
            _wheel.MarkDirtyRepaint();

            if (notify) OnHighlighted?.Invoke(_hiLevel, _hiIndex);
        }

        private void ApplyLabelClasses()
        {
            for (int k = 0; k < _rings.Count; k++)
                for (int i = 0; i < _rings[k].Count; i++)
                    _rings[k].Labels[i].EnableInClassList("radial-option--highlighted", IsLit(k, i));
        }

        /// <summary>
        /// Encendido: lo resaltado, y ademas el padre de cada anillo abierto, para que se vea
        /// de que opcion cuelga el anillo exterior.
        /// </summary>
        private bool IsLit(int level, int index)
        {
            if (level == _hiLevel && index == _hiIndex) return true;
            return level + 1 < _rings.Count && _rings[level + 1].Parent == index;
        }

        #endregion

        #region Dibujo

        private void OnStyleResolved(CustomStyleResolvedEvent evt)
        {
            ICustomStyle s = evt.customStyle;
            if (s.TryGetValue(FILL, out Color fill)) _fill = fill;
            if (s.TryGetValue(FILL_HIGHLIGHT, out Color fillHi)) _fillHighlight = fillHi;
            if (s.TryGetValue(STROKE, out Color stroke)) _stroke = stroke;
            if (s.TryGetValue(STROKE_HIGHLIGHT, out Color strokeHi)) _strokeHighlight = strokeHi;
            if (s.TryGetValue(STROKE_WIDTH, out float width)) _strokeWidth = width;
            if (s.TryGetValue(INNER_RADIUS, out float inner)) _inner = inner;
            if (s.TryGetValue(OUTER_RADIUS, out float outer)) _outer = outer;
            if (s.TryGetValue(GAP_DEGREES, out float gap)) _gapDegrees = gap;
            if (s.TryGetValue(RING_WIDTH, out float ringWidth)) _ringWidth = ringWidth;
            if (s.TryGetValue(RING_GAP, out float ringGap)) _ringGap = ringGap;
            if (s.TryGetValue(CHILD_SECTOR, out float childSector)) _childSectorDegrees = childSector;

            Relayout();
        }

        /// <summary>
        /// Un quesito por opcion y anillo: corona circular entre los radios del anillo,
        /// recortada a su porcion y con un hueco pequeno entre vecinos.
        ///
        /// <para>Painter2D mide los angulos desde el eje x y, con y hacia abajo, crece en
        /// sentido horario; aqui se miden desde "arriba". De ahi el -90 grados.</para>
        /// </summary>
        private void DrawSectors(MeshGenerationContext ctx)
        {
            if (_rings.Count == 0) return;

            Painter2D painter = ctx.painter2D;
            Vector2 center = Vector2.zero;
            float gap = _gapDegrees * Mathf.Deg2Rad * 0.5f;
            const float TOP = -Mathf.PI * 0.5f;

            for (int k = 0; k < _rings.Count; k++)
            {
                Ring ring = _rings[k];
                for (int i = 0; i < ring.Count; i++)
                {
                    float start = ring.Start + ring.Sector * i;
                    float a0 = TOP + start + gap;
                    float a1 = TOP + start + ring.Sector - gap;
                    bool lit = IsLit(k, i);

                    painter.BeginPath();
                    painter.Arc(center, ring.Outer, Angle.Radians(a0), Angle.Radians(a1), ArcDirection.Clockwise);
                    painter.Arc(center, ring.Inner, Angle.Radians(a1), Angle.Radians(a0), ArcDirection.CounterClockwise);
                    painter.ClosePath();

                    painter.fillColor = lit ? _fillHighlight : _fill;
                    painter.Fill();

                    painter.strokeColor = lit ? _strokeHighlight : _stroke;
                    painter.lineWidth = _strokeWidth;
                    painter.lineJoin = LineJoin.Round;
                    painter.Stroke();
                }
            }
        }

        #endregion
    }
}

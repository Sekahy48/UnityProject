using System;
using System.Collections.Generic;
using Core.MVC.View.UI.Radial;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVC.View.Controls
{
    /// <summary>
    /// Menu radial reutilizable de UI Toolkit: una rueda de quesitos alrededor del centro
    /// de la pantalla, elegidos por la direccion del puntero.
    ///
    /// <para><b>No sabe de que es el menu.</b> Recibe textos y avisa de indices: el mismo
    /// control servira el dia que el inventario quiera un radial. Toda la geometria sale de
    /// <see cref="RadialGeometry"/> (Core): los quesitos que se dibujan y la porcion que se
    /// elige son la misma cuenta.</para>
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

        private Color _fill = new Color(0.12f, 0.12f, 0.12f, 0.88f);
        private Color _fillHighlight = new Color(0.3f, 0.3f, 0.3f, 0.95f);
        private Color _stroke = new Color(0.6f, 0.6f, 0.6f, 0.9f);
        private Color _strokeHighlight = Color.white;
        private float _strokeWidth = 2f;
        private float _inner = 48f;
        private float _outer = 150f;
        private float _gapDegrees = 2f;

        #endregion

        /// <summary>
        /// Mas alla del borde exterior se sigue eligiendo un poco (un gesto rapido se pasa);
        /// a partir de aqui es "fuera": no resalta y un clic cierra.
        /// </summary>
        private const float OUTSIDE_FACTOR = 1.6f;

        public event Action<int> OnHighlighted;
        public event Action<int> OnClicked;
        public event Action OnDismissed;

        private readonly VisualElement _wheel;
        private readonly VisualElement _hub;
        private readonly List<Label> _labels = new List<Label>();
        private int _highlighted = -1;

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

        #region Abrir y cerrar

        public void Open(IReadOnlyList<string> labels)
        {
            foreach (Label label in _labels) label.RemoveFromHierarchy();
            _labels.Clear();

            for (int i = 0; i < labels.Count; i++)
            {
                Label label = new Label(labels[i]);
                label.AddToClassList("radial-option");
                if (i == 0) label.AddToClassList("radial-option--default");
                label.pickingMode = PickingMode.Ignore;
                _wheel.Add(label);
                _labels.Add(label);
            }

            PlaceLabels();

            _highlighted = -1;
            IsOpen = true;
            pickingMode = PickingMode.Position;
            style.display = DisplayStyle.Flex;
            _wheel.MarkDirtyRepaint();
        }

        public void Close()
        {
            IsOpen = false;
            pickingMode = PickingMode.Ignore;
            style.display = DisplayStyle.None;
            SetHighlighted(-1, notify: false);
        }

        /// <summary>Cada texto en el centro de su quesito: a medio camino entre los radios.</summary>
        private void PlaceLabels()
        {
            float mid = (_inner + _outer) * 0.5f;
            for (int i = 0; i < _labels.Count; i++)
            {
                (float x, float y) = RadialGeometry.OffsetOf(i, _labels.Count, mid);
                _labels[i].style.left = x;
                _labels[i].style.top = y;
            }
        }

        #endregion

        #region Puntero

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!IsOpen) return;
            SetHighlighted(IndexAt(evt.localPosition), notify: true);
        }

        /// <summary>
        /// Clic en un quesito: esa opcion. Clic en el centro o fuera: cerrar sin elegir. Se
        /// recalcula con la posicion del clic, por si llega sin un movimiento previo.
        /// </summary>
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!IsOpen) return;

            int index = IndexAt(evt.localPosition);
            evt.StopPropagation();

            if (index >= 0) OnClicked?.Invoke(index);
            else OnDismissed?.Invoke();
        }

        private int IndexAt(Vector3 local)
        {
            Rect r = contentRect;
            float dx = local.x - r.width * 0.5f;
            float dy = local.y - r.height * 0.5f;
            return RadialGeometry.IndexAt(dx, dy, _labels.Count, _inner, _outer * OUTSIDE_FACTOR);
        }

        private void SetHighlighted(int index, bool notify)
        {
            if (index == _highlighted) return;

            if (_highlighted >= 0 && _highlighted < _labels.Count)
                _labels[_highlighted].RemoveFromClassList("radial-option--highlighted");

            _highlighted = index;

            if (_highlighted >= 0 && _highlighted < _labels.Count)
                _labels[_highlighted].AddToClassList("radial-option--highlighted");

            _hub.EnableInClassList("radial-hub--idle", _highlighted < 0);
            _wheel.MarkDirtyRepaint();

            if (notify) OnHighlighted?.Invoke(_highlighted);
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

            PlaceLabels();
            _wheel.MarkDirtyRepaint();
        }

        /// <summary>
        /// Un quesito por opcion: corona circular entre el radio interior y el exterior,
        /// recortada a su porcion y con un hueco pequeno entre vecinos.
        ///
        /// <para>Painter2D mide los angulos desde el eje x y, con y hacia abajo, crece en
        /// sentido horario; RadialGeometry los mide desde "arriba". De ahi el -90 grados.</para>
        /// </summary>
        private void DrawSectors(MeshGenerationContext ctx)
        {
            int count = _labels.Count;
            if (count == 0) return;

            Painter2D painter = ctx.painter2D;
            Vector2 center = Vector2.zero;
            float gap = _gapDegrees * Mathf.Deg2Rad * 0.5f;
            const float TOP = -Mathf.PI * 0.5f;

            for (int i = 0; i < count; i++)
            {
                (float start, float end) = RadialGeometry.SectorOf(i, count);
                float a0 = TOP + start + gap;
                float a1 = TOP + end - gap;
                bool lit = i == _highlighted;

                painter.BeginPath();
                painter.Arc(center, _outer, Angle.Radians(a0), Angle.Radians(a1), ArcDirection.Clockwise);
                painter.Arc(center, _inner, Angle.Radians(a1), Angle.Radians(a0), ArcDirection.CounterClockwise);
                painter.ClosePath();

                painter.fillColor = lit ? _fillHighlight : _fill;
                painter.Fill();

                painter.strokeColor = lit ? _strokeHighlight : _stroke;
                painter.lineWidth = _strokeWidth;
                painter.lineJoin = LineJoin.Round;
                painter.Stroke();
            }
        }

        #endregion
    }
}

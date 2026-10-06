using System.Runtime.CompilerServices;
using Unity.Services;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace MVC.View
{
    public static class UIElementUtils
    {
        public static void AddAmountLabel(VisualElement element, int amount)
        {
            Label amountLabel = new Label(amount.ToString());
            amountLabel.AddToClassList("amount-label");
            amountLabel.pickingMode = PickingMode.Ignore;
            element.Add(amountLabel);
        }

        public static void SetBackgroundTexture(VisualElement element, string texturePath)
        {
            Texture2D tex = TextureCache.Instance.Get(texturePath);
            if (tex != null)
            {
                element.style.backgroundImage = new StyleBackground(tex);
                element.AddToClassList("icon-fit");
            } 
            else
                Debug.LogWarning($"No texture found at '{texturePath}'");
        }

        #region Colocar flotantes

        /// <summary>
        /// Coloca <paramref name="floating"/> (position: absolute) pegado a
        /// <paramref name="anchor"/> por el lado <paramref name="preferred"/>, y lo muestra.
        /// En cada eje, si por ese lado se saldria de <paramref name="frame"/>, pasa al
        /// contrario: en el eje del lado, al otro lado del ancla; en el otro eje, alineado por
        /// el borde opuesto. Si tampoco cabe asi, se queda donde caiga: eso es un problema de
        /// diseno de la interfaz, no algo que arreglar aqui. Ocultarlo es cosa de quien llama.
        ///
        /// <para>Hace falta su tamano para saber si cabe, y UI Toolkit no lo conoce hasta
        /// maquetarlo. Por eso se recoloca en cada GeometryChangedEvent del flotante (tambien
        /// cuando cambia el contenido y con el, el tamano), y mientras no tenga tamano se queda
        /// invisible para no verse un instante mal puesto. La decision sale del TAMANO del
        /// flotante y de las cajas del ancla y el marco, nunca de donde esta ahora: si saliera
        /// de su posicion, voltearlo dispararia otro GeometryChanged que lo volveria a mover
        /// (la misma leccion que el submenu contextual).</para>
        ///
        /// <para>El flotante no debe llevar translate en el USS: se coloca por su esquina
        /// superior izquierda.</para>
        /// </summary>
        /// <param name="gap">Separacion en pixeles entre el ancla y el flotante.</param>
        public static void PlaceNextTo(VisualElement floating, VisualElement anchor, VisualElement frame,
                                       Side preferred, float gap = 0f)
        {
            if (floating == null || anchor == null || frame == null || floating.parent == null) return;

            // Lo ultimo que se pidio; el manejador de geometria lee de aqui, asi que volver a
            // llamar solo actualiza los datos y no apila manejadores.
            bool firstTime = !Placements.TryGetValue(floating, out Placement placement);
            if (firstTime)
            {
                placement = new Placement();
                Placements.Add(floating, placement);
                floating.RegisterCallback<GeometryChangedEvent>(_ => Place(floating, Placements.TryGetValue(floating, out Placement p) ? p : null));
            }

            placement.Anchor = anchor;
            placement.Frame = frame;
            placement.Side = preferred;
            placement.Gap = gap;

            // Invisible hasta colocarlo: si aun no tiene tamano (venia oculto), lo pondra el
            // GeometryChanged que llegue tras maquetarlo, sin verse antes en el sitio viejo.
            floating.style.visibility = Visibility.Hidden;
            floating.style.display = DisplayStyle.Flex;
            Place(floating, placement);
        }

        /// <summary>Lado preferido para <see cref="PlaceNextTo"/>.</summary>
        public enum Side { Right, Left, Above, Below }

        private sealed class Placement
        {
            public VisualElement Anchor;
            public VisualElement Frame;
            public Side Side;
            public float Gap;
        }

        /* Datos de colocacion por flotante. Tabla debil: si el flotante desaparece (recarga de
           UI), su entrada se va con el y no hay que limpiar nada. */
        private static readonly ConditionalWeakTable<VisualElement, Placement> Placements = new();

        private static void Place(VisualElement floating, Placement placement)
        {
            if (placement == null || floating.parent == null) return;

            float w = floating.resolvedStyle.width;
            float h = floating.resolvedStyle.height;
            if (float.IsNaN(w) || float.IsNaN(h) || w <= 0f || h <= 0f)
            {
                // Aun sin maquetar: invisible hasta el GeometryChanged que traiga el tamano.
                floating.style.visibility = Visibility.Hidden;
                return;
            }

            Rect a = placement.Anchor.worldBound;
            Rect f = placement.Frame.worldBound;
            float gap = placement.Gap;
            float x, y;

            switch (placement.Side)
            {
                case Side.Left:
                    x = a.xMin - gap - w;
                    if (x < f.xMin) x = a.xMax + gap;
                    y = AlignStart(a.yMin, a.yMax, h, f.yMax);
                    break;
                case Side.Above:
                    y = a.yMin - gap - h;
                    if (y < f.yMin) y = a.yMax + gap;
                    x = AlignStart(a.xMin, a.xMax, w, f.xMax);
                    break;
                case Side.Below:
                    y = a.yMax + gap;
                    if (y + h > f.yMax) y = a.yMin - gap - h;
                    x = AlignStart(a.xMin, a.xMax, w, f.xMax);
                    break;
                default: // Right
                    x = a.xMax + gap;
                    if (x + w > f.xMax) x = a.xMin - gap - w;
                    y = AlignStart(a.yMin, a.yMax, h, f.yMax);
                    break;
            }

            Vector2 local = floating.parent.WorldToLocal(new Vector2(x, y));
            floating.style.left = local.x;
            floating.style.top = local.y;
            floating.style.visibility = Visibility.Visible;
        }

        /// <summary>
        /// En el eje que no es el del lado: alineado con el inicio del ancla (su borde
        /// izquierdo o superior); si asi se saldria por el final del marco, alineado con el
        /// final del ancla.
        /// </summary>
        private static float AlignStart(float anchorStart, float anchorEnd, float size, float frameEnd)
            => anchorStart + size > frameEnd ? anchorEnd - size : anchorStart;

        #endregion

        #region Sacudir

        /// <summary>
        /// Sacude el elemento en horizontal y lo deja donde estaba. Solo mueve su dibujo
        /// (translate), no la maquetacion: lo que tiene alrededor no se recoloca. Si ya se
        /// estaba sacudiendo, empieza de nuevo.
        ///
        /// <para>Anima un valor de 0 a 1 en <paramref name="durationMs"/> y en cada fotograma
        /// lo convierte en desplazamiento: un seno con <paramref name="oscillations"/> vaivenes,
        /// escalado a <paramref name="amplitude"/> pixeles y apagado por (1 - t), para que acabe
        /// suave y no de golpe. Al terminar se quita el translate y manda el del USS.</para>
        /// </summary>
        public static void Shake(VisualElement element, int durationMs = 1000,
                                 float amplitude = 4f, int oscillations = 6)
        {
            if (element == null) return;

            // Dos animaciones sobre el mismo translate se pelearian: la anterior se para.
            if (Shakes.TryGetValue(element, out ShakeHandle previous))
            {
                previous.Animation?.Stop();
                Shakes.Remove(element);
            }

            ShakeHandle handle = new ShakeHandle();
            Shakes.Add(element, handle);

            handle.Animation = element.experimental.animation
                .Start(0f, 1f, durationMs, (target, t) =>
                {
                    float offset = amplitude * Mathf.Sin(2f * Mathf.PI * oscillations * t) * (1f - t);
                    target.style.translate = new Translate(offset, 0f);
                })
                .Ease(Easing.Linear)
                .OnCompleted(() =>
                {
                    element.style.translate = StyleKeyword.Null;
                    if (Shakes.TryGetValue(element, out ShakeHandle current) && current == handle)
                        Shakes.Remove(element);
                });
        }

        private sealed class ShakeHandle
        {
            public ValueAnimation<float> Animation;
        }

        /* Sacudida en curso por elemento, para poder pararla si llega otra. Tabla debil, como
           Placements: se va con el elemento. */
        private static readonly ConditionalWeakTable<VisualElement, ShakeHandle> Shakes = new();

        #endregion
    }
}
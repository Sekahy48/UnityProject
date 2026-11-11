using Core.Inventory;
using NUnit.Framework;

namespace Core.Tests
{
    /// <summary>A. La rejilla (TetrisGridState): colocar, rechazar, buscar hueco, liberar, intercambiar.</summary>
    public class GridTests
    {
        private static ItemObject Node(int w, int h) => new ItemObject(TestItems.Item(TestItems.SWORD, maxStack: 1, w: w, h: h), 1);

        [Test]
        public void Colocar_un_bloque_de_2x3_ocupa_sus_seis_celdas()
        {
            TetrisGridState grid = new TetrisGridState(4, 4);
            ItemObject node = Node(w: 2, h: 3);

            Assert.IsTrue(grid.Place(node, new GridPos(0, 1)));

            for (int r = 0; r < 3; r++)
                for (int c = 1; c < 3; c++)
                    Assert.AreEqual(node.GetNodeId(), grid.GetCellAt(new GridPos(r, c)), $"celda ({r},{c})");
            Assert.AreEqual(16 - 6, grid.GetFreeCellCount());
        }

        [Test]
        public void Colocar_encima_de_otro_bloque_se_rechaza_y_no_toca_nada()
        {
            TetrisGridState grid = new TetrisGridState(4, 4);
            grid.Place(Node(2, 2), new GridPos(0, 0));

            Assert.IsFalse(grid.Place(Node(2, 2), new GridPos(1, 1)));
            Assert.AreEqual(16 - 4, grid.GetFreeCellCount());
        }

        [Test]
        public void Colocar_saliendose_de_la_rejilla_se_rechaza()
        {
            TetrisGridState grid = new TetrisGridState(4, 4);

            Assert.IsFalse(grid.Place(Node(2, 2), new GridPos(3, 3)));   // se sale por abajo y por la derecha
            Assert.IsFalse(grid.Place(Node(1, 1), new GridPos(-1, 0)));
            Assert.AreEqual(16, grid.GetFreeCellCount());
        }

        [Test]
        public void El_primer_hueco_se_busca_de_izquierda_a_derecha_y_de_arriba_abajo()
        {
            TetrisGridState grid = new TetrisGridState(3, 4);
            grid.Place(Node(2, 2), new GridPos(0, 0));

            Assert.AreEqual(new GridPos(0, 2), grid.FindFirstFit(2, 2));   // a la derecha del bloque
            Assert.AreEqual(new GridPos(0, 2), grid.FindFirstFit(1, 1));

            grid.Place(Node(2, 1), new GridPos(0, 2));                     // fila 0 llena
            Assert.AreEqual(new GridPos(1, 2), grid.FindFirstFit(1, 1));   // salta a la fila de abajo
        }

        [Test]
        public void Sin_hueco_para_el_tamano_pedido_no_hay_posicion()
        {
            TetrisGridState grid = new TetrisGridState(2, 2);
            Assert.IsTrue(grid.FindFirstFit(3, 1).IsNone);
        }

        [Test]
        public void Quitar_un_bloque_libera_sus_celdas()
        {
            TetrisGridState grid = new TetrisGridState(4, 4);
            ItemObject node = Node(2, 2);
            grid.Place(node, new GridPos(1, 1));

            Assert.IsTrue(grid.Remove(node.GetNodeId()));

            Assert.AreEqual(16, grid.GetFreeCellCount());
            Assert.IsNull(grid.GetElementOf(node.GetNodeId()));
        }

        [Test]
        public void Intercambiar_deja_cada_bloque_en_las_celdas_del_otro()
        {
            TetrisGridState grid = new TetrisGridState(3, 3);
            ItemObject a = Node(1, 1);
            ItemObject b = Node(1, 1);
            grid.Place(a, new GridPos(0, 0));
            grid.Place(b, new GridPos(2, 2));

            Assert.IsTrue(grid.SwapNodes(a, new GridPos(2, 2), b));

            Assert.AreEqual(new GridPos(2, 2), grid.GetElementOf(a.GetNodeId()).GetPos());
            Assert.AreEqual(new GridPos(0, 0), grid.GetElementOf(b.GetNodeId()).GetPos());
        }
    }
}

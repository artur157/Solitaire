using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kosynka
{
    public partial class Form1 : Form
    {
        public const int wCard = 100, hCard = 140;                  // размер карты
        public const int wOffset = wCard / 8, hOffset = hCard / 8;  // расстояния между картами
        public const int wShift = wCard / 5, hShift = hCard / 5;
        public const int wField = (wCard + wOffset) * 8 + wOffset, hField = (hCard + hOffset) * 5, wWindow = wField + 17, offset = 25, hWindow = hField + 40 + offset;  // учитываем края

        public int time = 0;
        bool win = false;
        int accent = -1, accent2 = -1, accentLen = 1;

        int oldex, oldey, x, y;    // координаты перемещаемой фигуры
        int candOldPlace;          // каждое место имеет свой код от 0 до 15
        bool dragging = false;

        List<int> oldPlace, newPlace, countRemember;  // кол-во переложенных карт (вместо памятного буфера)

        List<Card>[] stacks;       // ну угадайте что это
        List<Card>[] stacksReady;
        Card[] cells;

        List<Card> buffer;         // для переноса

        Random rnd = new Random();
        int[] used;                // нужно при растасовке карт
        bool left = true;

        public Form1()
        {
            InitializeComponent();
            this.Width = wWindow;
            this.Height = hWindow;

            stacks = new List<Card>[8];
            stacksReady = new List<Card>[4];

            for (int i = 0; i < 8; ++i)
            {
                stacks[i] = new List<Card>();
                stacks[i].Clear();
            }

            for (int i = 0; i < 4; ++i)
            {
                stacksReady[i] = new List<Card>();
                stacksReady[i].Clear();
            }

            buffer = new List<Card>();

            cells = new Card[4];

            oldPlace = new List<int>();
            newPlace = new List<int>();
            countRemember = new List<int>();

            used = new int[52];
            for (int i = 0; i < 52; ++i)
            {
                used[i] = i;
            }

            Shuffle();
            FillStacksAndList();
        }

        public void swap(ref int a, ref int b)
        {
            int tmp = a;
            a = b;
            b = tmp;
        }

        private void Shuffle()
        {
            for (int i = 0; i < 52; ++i)
            {
                swap(ref used[i], ref used[rnd.Next(0, 52)]);
            }
        }

        private void FillStacksAndList()
        {
            int index = 0;

            for (int i = 0; i < 8; ++i)
            {
                for (int j = 0; j < 7 - (i + 4) / 8; ++j)
                {
                    stacks[i].Add(new Card(used[index] / 13, used[index] % 13 + 1, true));
                    ++index;
                }
            }
        }

        private String GetNameOfPic(Card card)
        {
            String s = "";
            if (card.opened)
            {
                switch (card.suit)
                {
                    case 0: s += "clubs"; break;
                    case 1: s += "hearts"; break;
                    case 2: s += "spades"; break;
                    case 3: s += "diamonds"; break;
                }
                s += card.number;
            }
            else
            {
                s = "shirt" + Data.numShirt;
            }

            return s;
        }

        public static bool IsIn(int x, int y, int start, int start2, int len, int len2)   // находится в прямоугольнике (с учетом offset)
        {
            return x >= start && x <= start + len && y >= start2 && y <= start2 + len2;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen pen = new Pen(Color.Black);
            Pen light_green_pen = new Pen(Color.LightGreen);
            Pen dark_green_pen = new Pen(Color.Black);
            Pen yellow_pen = new Pen(Color.Yellow, 5);
            Brush bg_brush = new SolidBrush(Color.Green);
            Brush darkgreen_brush = new SolidBrush(Color.DarkGreen);
            Image image = (Image)Kosynka.Properties.Resources.ResourceManager.GetObject("soliter1");

            // портрет
            if (left)
            {
                image = (Image)Kosynka.Properties.Resources.ResourceManager.GetObject("soliter2");
            }
            else
            {
                image = (Image)Kosynka.Properties.Resources.ResourceManager.GetObject("soliter1");
            }
            g.DrawImage(image, wOffset / 2 + (wOffset + wCard) * 4 - 30, offset + hOffset + 20, 60, 60);

            // линии
            for (int i = 0; i < 4; i++)
            {
                g.DrawLine(dark_green_pen, i * wCard, offset, (i + 1) * wCard, offset);
                g.DrawLine(dark_green_pen, i * wCard, offset, i * wCard, offset + hCard);
                g.DrawLine(light_green_pen, i * wCard, offset + hCard, (i + 1) * wCard, offset + hCard);
                g.DrawLine(light_green_pen, (i + 1) * wCard - 1, offset, (i + 1) * wCard - 1, offset + hCard);
            }
            for (int i = 0; i < 4; i++)
            {
                g.DrawLine(dark_green_pen, wField - 4 * wCard + i * wCard, offset, wField - 4 * wCard + (i + 1) * wCard, offset);
                g.DrawLine(dark_green_pen, wField - 4 * wCard + i * wCard, offset, wField - 4 * wCard + i * wCard, offset + hCard);
                g.DrawLine(light_green_pen, wField - 4 * wCard + i * wCard, offset + hCard, wField - 4 * wCard + (i + 1) * wCard, offset + hCard);
                g.DrawLine(light_green_pen, wField - 4 * wCard + (i + 1) * wCard - 1, offset, wField - 4 * wCard + (i + 1) * wCard - 1, offset + hCard);
            }

            // ячейки
            for (int i = 0; i < 4; i++)
            {
                if (cells[i] != null)
                {
                    String s = GetNameOfPic(cells[i]);
                    image = (Image)Kosynka.Properties.Resources.ResourceManager.GetObject(s);
                    g.DrawImage(image, i * wCard, offset, wCard, hCard);
                }
            }

            // пазы
            for (int i = 0; i < 4; i++)
            {
                if (stacksReady[i].Count > 0)
                {
                    String s = GetNameOfPic(stacksReady[i][stacksReady[i].Count - 1]);
                    image = (Image)Kosynka.Properties.Resources.ResourceManager.GetObject(s);
                    g.DrawImage(image, wField - 4 * wCard + i * wCard, offset, wCard, hCard);
                }
            }

            // отображаем стеки
            for (int i = 0; i < 8; i++)
            {
                if (stacks[i].Count == 0)
                {
                    /*g.FillRectangle(darkgreen_brush, wOffset + (wOffset + wCard) * i, offset + hOffset * 2 + hCard, wCard, hCard);
                    g.DrawRectangle(pen, wOffset + (wOffset + wCard) * i, offset + hOffset * 2 + hCard, wCard, hCard);*/
                }
                else
                {
                    for (int j = 0; j < stacks[i].Count; j++)
                    {
                        String s = GetNameOfPic(stacks[i][j]);
                        image = (Image)Kosynka.Properties.Resources.ResourceManager.GetObject(s);
                        g.DrawImage(image, wOffset + (wOffset + wCard) * i, offset + hOffset * 2 + hCard + hShift * j, wCard, hCard);
                    }
                }
            }

            if (dragging)
            {
                String s;

                for (int i = 0; i < buffer.Count; i++)
                {
                    if (buffer[i] != null)
                    {
                        s = GetNameOfPic(buffer[i]);
                        image = (Image)Kosynka.Properties.Resources.ResourceManager.GetObject(s);
                        g.DrawImage(image, x, y + hShift * i, wCard, hCard);
                    }
                }

            }

            // для случая подсказки
            if (accent > -1)
            {
                if (accent < 8)
                {
                    g.DrawRectangle(yellow_pen, wOffset + (wOffset + wCard) * accent, offset + hOffset * 2 + hCard + hShift * ((stacks[accent].Count > 0 ? stacks[accent].Count : stacks[accent].Count + 1) - 1) - (accentLen - 1) * hShift, wCard, hCard + (accentLen - 1) * hShift);
                }
                else if (accent < 12)
                {
                    g.DrawRectangle(yellow_pen, wField - 4 * wCard + (accent - 8) * wCard, offset, wCard, hCard);
                }
                else
                {
                    g.DrawRectangle(yellow_pen, wCard * (accent - 12), offset, wCard, hCard);
                }
            }

            if (accent2 > -1)
            {
                //MessageBox.Show("" + accent2);
                if (accent2 < 8)
                {
                    g.DrawRectangle(yellow_pen, wOffset + (wOffset + wCard) * accent2, offset + hOffset * 2 + hCard + hShift * ((stacks[accent2].Count > 0 ? stacks[accent2].Count : stacks[accent2].Count + 1) - 1), wCard, hCard);
                }
                else if (accent2 < 12)
                {
                    g.DrawRectangle(yellow_pen, wField - 4 * wCard + (accent2 - 8) * wCard, offset, wCard, hCard);
                }
                else
                {
                    g.DrawRectangle(yellow_pen, wCard * (accent2 - 12), offset, wCard, hCard);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ++time;
            toolStripStatusLabel1.Text = "Время: " + time;
        }

        public class Card
        {
            public int suit;   // 1 - clubs, 2 - hearts, 3 - spades, 4 - diamonds
            public int number; // 11 - валет, 12 - дама, 13 - король
            public bool opened;

            public Card(int suit, int number, bool opened = false)
            {
                this.suit = suit;
                this.number = number;
                this.opened = opened;
            }
        }

        bool isNull(Card card)
        {
            return card == null;
        }

        void ClearNulls(List<Card> list)
        {
            list.RemoveAll(isNull);
        }

        bool TryStroke(MouseEventArgs e)  // пытаемся сделать ход 
        {
            // если координаты сейчас на прямоугольнике, проверяем, можно ли так, и если да, то переносим с буфера туда
            // проверяем стеки
            for (int i = 0; i < 8; i++)
            {
                int j = stacks[i].Count - 1;

                if (IsIn(e.X, e.Y, wOffset + (wOffset + wCard) * i, offset + hOffset * 2 + hCard + hShift * j, wCard, hCard))
                {
                    if (stacks[i].Count == 0 || condNorm(stacks[i][stacks[i].Count - 1], buffer[0]))
                    {
                        countRemember.Add(buffer.Count);   // для отмены хода
                        oldPlace.Add(candOldPlace);
                        newPlace.Add(i);
                        ToNewPlace();

                        отменитьХодToolStripMenuItem.Enabled = true;

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }

            // проверяем пазы
            for (int i = 0; i < 4; i++)
            {
                int j = stacks[i].Count - 1;

                if (IsIn(e.X, e.Y, wField - 4 * wCard + i * wCard, offset, wCard, hCard))
                {
                    if (buffer.Count == 1 && (stacksReady[i].Count == 0 && buffer[0].number == 1 || stacksReady[i].Count > 0 && condNext(stacksReady[i][stacksReady[i].Count - 1], buffer[0])))
                    {
                        countRemember.Add(1);   // для отмены хода
                        oldPlace.Add(candOldPlace);
                        newPlace.Add(i + 8);
                        ToNewPlace();

                        отменитьХодToolStripMenuItem.Enabled = true;

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }

            // проверяем ячейки  
            for (int i = 0; i < 4; i++)
            {
                if (cells[i] == null && buffer.Count == 1 && IsIn(e.X, e.Y, i * wCard, offset, wCard, hCard))
                {
                    countRemember.Add(1);   // для отмены хода
                    oldPlace.Add(candOldPlace);
                    newPlace.Add(i + 12);
                    ToNewPlace();

                    отменитьХодToolStripMenuItem.Enabled = true;

                    return true;
                }
            }

            return false;
        }

        void Put(int place)
        {
            if (place >= 12)
            {
                cells[place - 12] = buffer[0];
            }
            else if (place >= 8)
            {
                stacksReady[place - 8].Add(buffer[0]);
            }
            else
            {
                for (int i = 0; i < buffer.Count; i++)
                {
                    stacks[place].Add(buffer[i]);
                }
            }
        }

        void GetBack()   // возврат из буфера на место
        {
            Put(oldPlace[oldPlace.Count - 1]);
        }

        void ToNewPlace()  // кладем на новое место
        {
            Put(newPlace[newPlace.Count - 1]);
        }

        bool condNorm(Card c1, Card c2)   // условие для буфера, чтоб можно было карты друг на друга в стеке класть
        {
            return c1.number - c2.number == 1 && (c1.suit + c2.suit) % 2 == 1;
        }

        bool condNext(Card c1, Card c2)   // условие для пазов, чтоб после червового туза можно было класть только червовую двойку
        {
            return c2.number - c1.number == 1 && c1.suit == c2.suit;
        }

        bool normBuffer()
        {
            for (int i = 0; i < buffer.Count - 1; i++)
            {
                if (!condNorm(buffer[i], buffer[i + 1]))
                {
                    return false;
                }
            }
            return true;
        }

        bool Win()    // критерий победы
        {
            for (int i = 0; i < 4; i++)
            {
                if (stacksReady[i].Count != 13)
                {
                    return false;
                }
            }
            return true;
        }

        bool StacksEmpty()
        {
            return stacksReady[0].Count == 0 && stacksReady[1].Count == 0 && stacksReady[2].Count == 0 && stacksReady[3].Count == 0;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!win)
            {
                accent = -1;
                accent2 = -1;

                if (e.Clicks == 2)
                {
                    // проверяем стеки
                    for (int i = 0; i < 8; i++)
                    {
                        int j = stacks[i].Count - 1;

                        if (stacks[i].Count > 0 && IsIn(e.X, e.Y, wOffset + (wOffset + wCard) * i, offset + hOffset * 2 + hCard + hShift * j, wCard, hCard) /*&& stacks[i][j].opened*/)
                        {
                            for (int k = 0; k < 4; k++)
                            {
                                if (stacksReady[k].Count == 0 && stacks[i][stacks[i].Count - 1].number == 1 || stacksReady[k].Count > 0 && condNext(stacksReady[k][stacksReady[k].Count - 1], stacks[i][stacks[i].Count - 1]))
                                {
                                    oldPlace.Add(i);
                                    countRemember.Add(1);   // для отмены хода
                                    buffer.Add(stacks[i][stacks[i].Count - 1]);
                                    stacks[i].RemoveAt(stacks[i].Count - 1);

                                    newPlace.Add(k + 8);
                                    ToNewPlace();

                                    отменитьХодToolStripMenuItem.Enabled = true;

                                    return;
                                }
                            }

                            // вот тут в ячейку попробовать отправить
                            for (int k = 0; k < 4; k++)
                            {
                                if (cells[k] == null)
                                {
                                    oldPlace.Add(i);

                                    countRemember.Add(1);   // для отмены хода
                                    buffer.Add(stacks[i][stacks[i].Count - 1]);
                                    stacks[i].RemoveAt(stacks[i].Count - 1);

                                    newPlace.Add(k + 12);
                                    ToNewPlace();

                                    отменитьХодToolStripMenuItem.Enabled = true;

                                    return;
                                }
                            }
                        }

                    }

                    // проверяем ячейки
                    for (int i = 0; i < 4; i++)
                    {
                        if (cells[i] != null && IsIn(e.X, e.Y, wCard * i, offset, wCard, hCard))
                        {
                            for (int k = 0; k < 4; k++)
                            {
                                if (stacksReady[k].Count == 0 && cells[i].number == 1 || stacksReady[k].Count > 0 && condNext(stacksReady[k][stacksReady[k].Count - 1], cells[i]))
                                {
                                    oldPlace.Add(i + 12);
                                    countRemember.Add(1);   // для отмены хода
                                    buffer.Add(cells[i]);
                                    cells[i] = null;

                                    newPlace.Add(k + 8);
                                    ToNewPlace();

                                    отменитьХодToolStripMenuItem.Enabled = true;

                                    /*break;*/return;
                                }
                            }

                            return;
                        }
                    }

                    //Form1_MouseUp(sender, e);
                }

                if (e.Clicks == 1)
                {
                    oldex = e.X;
                    oldey = e.Y;

                    // проверяем стеки
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = stacks[i].Count - 1; j >= 0; j--)
                        {
                            if (IsIn(e.X, e.Y, wOffset + (wOffset + wCard) * i, offset + hOffset * 2 + hCard + hShift * j, wCard, hCard) && stacks[i][j].opened)
                            {
                                int k = j;
                                while (k < stacks[i].Count)
                                {
                                    buffer.Add(stacks[i][k]);
                                    stacks[i].RemoveAt(k);
                                }

                                candOldPlace = i;
                                dragging = normBuffer();
                                if (!normBuffer())
                                {
                                    Put(candOldPlace);
                                    return;
                                }
                                x = wOffset + (wOffset + wCard) * i;
                                y = offset + hOffset * 2 + hCard + hShift * j;
                                /*break;*/return;
                            }
                        }
                    }

                    // проверяем пазы
                    for (int i = 0; i < 4; i++)
                    {
                        int j = stacksReady[i].Count - 1;

                        if (IsIn(e.X, e.Y, wField - 4 * wCard + i * wCard, offset, wCard, hCard) && stacksReady[i].Count > 0)
                        {
                            buffer.Add(stacksReady[i][j]);
                            stacksReady[i].RemoveAt(j);

                            candOldPlace = i + 8;
                            dragging = true;

                            x = wField - 4 * wCard + i * wCard;
                            y = offset;
                            /*break;*/return;
                        }
                    }

                    // проверяем ячейки
                    for (int i = 0; i < 4; i++)
                    {
                        if (IsIn(e.X, e.Y, wCard * i, offset, wCard, hCard) && cells[i] != null)
                        {
                            buffer.Add(cells[i]);
                            cells[i] = null;

                            candOldPlace = i + 12;
                            dragging = true;

                            x = wCard * i;
                            y = offset;
                            /*break;*/return;
                        }
                    }
                }

            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                if (!TryStroke(e))
                    Put(candOldPlace);
            }
            dragging = false;
            buffer.Clear();

            if (!win && Win())
            {
                win = true;
                подсказкаToolStripMenuItem.Enabled = false;
                отменитьХодToolStripMenuItem.Enabled = false;
                Invalidate();
                MessageBox.Show("Поздравляем, вы выиграли!");
            }

            Invalidate();
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                x += e.X - oldex;
                y += e.Y - oldey;
                Invalidate();
                oldex = e.X;
                oldey = e.Y;
            }

            left = e.X < wOffset / 2 + (wOffset + wCard) * 4;

            Invalidate();
        }

        private void новаяИграToolStripMenuItem_Click(object sender, EventArgs e)
        {
            time = 0;
            win = false;
            dragging = false;
            accent = -1;
            accent2 = -1;

            for (int i = 0; i < 8; ++i)
            {
                stacks[i].Clear();
            }

            for (int i = 0; i < 4; ++i)
            {
                stacksReady[i].Clear();
            }

            buffer.Clear();

            oldPlace.Clear();
            newPlace.Clear();
            countRemember.Clear();

            for (int i = 0; i < 52; ++i)
            {
                used[i] = i;
            }

            for (int i = 0; i < 4; i++)
            {
                cells[i] = null;
            }

            Shuffle();
            FillStacksAndList();

            отменитьХодToolStripMenuItem.Enabled = false;
            подсказкаToolStripMenuItem.Enabled = true;
            timer1.Start();

            Invalidate();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void справкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Пасьянс \"Солитер\"\nWindows XP\n\nПрограммист: Гумеров Артур", "Справка");
        }

        private void отменитьХодToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!dragging)
            {
                accent = -1;
                accent2 = -1;

                int last = oldPlace.Count - 1;

                // переложить countRemember карт с newPlace на oldPlace

                // I. переложить countRemember карт с newPlace на buffer

                for (int i = 0; i < countRemember[last]; i++)
                {
                    // сначала удаляем из нового места
                    Card card;
                    if (newPlace[last] < 8)
                    {
                        card = stacks[newPlace[last]][stacks[newPlace[last]].Count - 1];
                        stacks[newPlace[last]].RemoveAt(stacks[newPlace[last]].Count - 1);
                    }
                    else if (newPlace[last] < 12)
                    {
                        card = stacksReady[newPlace[last] - 8][stacksReady[newPlace[last] - 8].Count - 1];    // где тут выход за границы? 
                        stacksReady[newPlace[last] - 8].RemoveAt(stacksReady[newPlace[last] - 8].Count - 1);
                    }
                    else
                    {
                        card = cells[newPlace[last] - 12];
                        cells[newPlace[last] - 12] = null;
                    }

                    // затем в буфер
                    buffer.Add(card);
                }

                // II. переложить countRemember карт с buffer на oldPlace

                for (int i = 0; i < countRemember[last]; i++)
                {
                    // сначала удаляем из буфера
                    Card card = buffer[buffer.Count - 1];
                    buffer.RemoveAt(buffer.Count - 1);

                    // затем возвращаем в старое
                    if (oldPlace[last] < 8)
                    {
                        stacks[oldPlace[last]].Add(card);
                    }
                    else if (oldPlace[last] < 12)
                    {
                        stacksReady[oldPlace[last] - 8].Add(card);
                    }
                    else
                    {
                        cells[oldPlace[last] - 12] = card;
                    }
                }

                oldPlace.RemoveAt(last);
                newPlace.RemoveAt(last);
                countRemember.RemoveAt(last);

                if (oldPlace.Count == 0)
                {
                    отменитьХодToolStripMenuItem.Enabled = false;
                }

                Invalidate();
            }
        }

        bool normStack(int number)
        {
            int pos = 0;
            while (!stacks[number][pos].opened)
                ++pos;

            for (int i = pos; i < stacks[number].Count - 1; i++)
            {
                if (!condNorm(stacks[number][i], stacks[number][i + 1]))
                {
                    return false;
                }
            }

            return true;
        }

        int normStackNumber(int number)  // всегда хотя бы один элемент в стеке будет норм
        {
            bool uslNorm = true;
            int pos = 0;
            while (!stacks[number][pos].opened)
                ++pos;

            for (int i = pos; i < stacks[number].Count - 1; i++)
            {
                uslNorm = true;

                for (int j = i; j < stacks[number].Count - 1; j++)
                {
                    if (!condNorm(stacks[number][j], stacks[number][j + 1]))
                    {
                        uslNorm = false;
                    }
                }

                if (uslNorm)
                {
                    return i;
                }
            }

            return stacks[number].Count - 1;
        }

        private void подсказкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            accentLen = 1;

            // смотрим где что подходит
            // сначала пытаемся выложить на стекреди что-нибудь
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (cells[j] != null && (stacksReady[i].Count == 0 && cells[j].number == 1 || stacksReady[i].Count > 0 && condNext(stacksReady[i][stacksReady[i].Count - 1], cells[j])))
                    {
                        accent = j + 12;
                        accent2 = i + 8;
                        Invalidate();
                        return;
                    }
                }
            }
            
            for (int j = 0; j < 8; j++)
            {
                for (int i = 0; i < 4; i++)
			    {
                    if (stacks[j].Count > 0 && (stacksReady[i].Count == 0 && stacks[j][stacks[j].Count - 1].number == 1 || stacksReady[i].Count > 0 && condNext(stacksReady[i][stacksReady[i].Count - 1], stacks[j][stacks[j].Count - 1])))
                    {
                        accent = j;
                        accent2 = i + 8;
                        Invalidate();
                        return;
                    }
			    }
            }

            // потом хотим переложить с ячеек на стеки, куда подходит?
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (cells[j] != null && (stacks[i].Count == 0 || stacks[i].Count > 0 && condNorm(stacks[i][stacks[i].Count - 1], cells[j])))
                    {
                        accent = j + 12;
                        accent2 = i;
                        Invalidate();
                        return;
                    }
                }
            }

            // потом со стека на стек хотим перекладывать (с j на i)
            /*for (int j = 0; j < 8; j++)
                for (int i = 0; i < 8; i++)
                {
                    int k = 0;
                    while (k < stacks[j].Count && !stacks[j][k].opened) ++k;

                    if (i != j && stacks[j].Count > 0 && (stacks[i].Count == 0 || stacks[i].Count > 0 && condNorm(stacks[i][stacks[i].Count - 1], stacks[j][k])))
                    {
                        accentLen = stacks[j].Count - k;
                        if (stacks[i].Count == 0 && k == 0) continue;
                        accent = j;
                        accent2 = i;
                        Invalidate();
                        return;
                    }
                }*/

            // потом со стека на стек хотим перекладывать (с j на i)
            for (int j = 0; j < 8; j++)
                if (stacks[j].Count > 0)
                    for (int i = 0; i < 8; i++)
                    {
                        int k = 0;
                        while (k < stacks[j].Count && !stacks[j][k].opened) ++k;

                        if (i != j && stacks[j].Count > 0 && (stacks[i].Count == 0 || stacks[i].Count > 0 && condNorm(stacks[i][stacks[i].Count - 1], stacks[j][k])) && normStack(j))
                        {
                            accentLen = stacks[j].Count - k;
                            if (stacks[i].Count == 0 && k == 0) continue;
                            accent = j;
                            accent2 = i;
                            Invalidate();
                            return;
                        }
                    }

            // идеально не получилось, давайте попроще
            for (int j = 0; j < 8; j++)
                if (stacks[j].Count > 0)
                    for (int i = 0; i < 8; i++)
                    {
                        int k = normStackNumber(j);
                        //while (k < stacks[j].Count && !stacks[j][k].opened) ++k;

                        if (i != j && stacks[j].Count > 0 && (stacks[i].Count == 0 || stacks[i].Count > 0 && condNorm(stacks[i][stacks[i].Count - 1], stacks[j][k])))
                        {
                            accentLen = stacks[j].Count - k;
                            if (stacks[i].Count == 0 && k == 0) continue;
                            accent = j;
                            accent2 = i;
                            Invalidate();
                            return;
                        }
                    }

            accent = -1;
            accent2 = -1;
            Invalidate();
        }
    }
}

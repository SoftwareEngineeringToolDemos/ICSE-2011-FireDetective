using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace FireDetectiveAnalyzer
{
    public partial class CodeView : UserControl
    {
        private TextDocument m_Document;
        private int m_CurrentLine = 0;
        private int m_CurrentColumn = 0;
        private int m_NumLines = 0;
        private int m_NumColumns = 0;
        private SizeF m_CharSize;
        private SizeF m_TabCharSize;
        private Font m_BoldFont;

        private List<Brush> m_Formats = new List<Brush>();
        private List<FormattedTextLayer> m_FormatLayers = new List<FormattedTextLayer>();
        private FormattedEntity m_SelectedEntity;

        public event EventHandler<CodeViewEventArgs> HighlightEntityUsage;

        public CodeView()
        {
            SetStyle(ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint, true);
            CalculateCharSize();
            InitializeComponent();
            CalculateNumLinesColumns();

            m_Formats.Add(null);
            m_Formats.Add(Brushes.Black);
            m_Formats.Add(Brushes.White);
            m_Formats.Add(SystemBrushes.Highlight);

            /*m_Formats.Add(new SolidBrush(Color.FromArgb(255, 255, 255)));            
            m_Formats.Add(new SolidBrush(Color.FromArgb(255, 255, 225)));
            m_Formats.Add(new SolidBrush(Color.FromArgb(235, 255, 255)));
            m_Formats.Add(Brushes.White);
            m_Formats.Add(new SolidBrush(Color.FromArgb(128, 224, 255)));
            m_Formats.Add(new SolidBrush(Color.FromArgb(225, 255, 255)));*/

            m_Formats.Add(Brushes.White);
            m_Formats.Add(Brushes.White);
            m_Formats.Add(new SolidBrush(Color.FromArgb(255, 255, 160)));
            m_Formats.Add(Brushes.White);
            m_Formats.Add(new SolidBrush(Color.FromArgb(128, 144, 255)));
            m_Formats.Add(new SolidBrush(Color.FromArgb(192, 192, 255)));

            m_Formats.Add(Brushes.Green); // 10 - 18 = JavaScriptTokenType + 10
            m_Formats.Add(Brushes.Green);
            m_Formats.Add(Brushes.Black);
            m_Formats.Add(Brushes.Blue);
            m_Formats.Add(new SolidBrush(Color.FromArgb(80, 80, 80)));
            m_Formats.Add(Brushes.Red);
            m_Formats.Add(Brushes.Magenta);
            m_Formats.Add(Brushes.Red);
            m_Formats.Add(Brushes.Black);
            m_Formats.Add(null);

            m_Formats.Add(Brushes.Green); // 20 - 27 = JavaTokenType + 20
            m_Formats.Add(Brushes.Green);
            m_Formats.Add(Brushes.Black);
            m_Formats.Add(new SolidBrush(Color.FromArgb(127, 0, 127)));
            m_Formats.Add(new SolidBrush(Color.FromArgb(80, 80, 80)));
            m_Formats.Add(Brushes.Blue);
            m_Formats.Add(Brushes.Blue);
            m_Formats.Add(Brushes.Black);
            m_Formats.Add(null);
            m_Formats.Add(null);

            m_Formats.Add(new SolidBrush(Color.FromArgb(240, 240, 240))); // 30 = HtmlDocument non-js background
            m_Formats.Add(null);
            m_Formats.Add(null);
            m_Formats.Add(null);
            m_Formats.Add(null);
            m_Formats.Add(null);
            m_Formats.Add(null);
            m_Formats.Add(null);
            m_Formats.Add(null);
            m_Formats.Add(null);

            m_Formats.Add(Brushes.Black); // 40 - 43 = JspTokenType + 40
            m_Formats.Add(Brushes.Green);
            m_Formats.Add(null);
            m_Formats.Add(null);
            m_Formats.Add(Brushes.Blue);
            m_Formats.Add(null);
            m_Formats.Add(null);
            m_Formats.Add(null);
            m_Formats.Add(null);
            m_Formats.Add(null);

            ResetFormatLayers();

            FormattedDocument = null;
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            SetBackColorFormat();
            base.OnBackColorChanged(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            CalculateCharSize();
            m_BoldFont = new Font(Font, FontStyle.Bold);
            base.OnFontChanged(e);
        }

        protected override void OnResize(EventArgs e)
        {
            CalculateNumLinesColumns();
            base.OnResize(e);
        }

        private void CalculateNumLinesColumns()
        {
            m_NumLines = Math.Max(1, (int)(TextAreaSize.Height / m_CharSize.Height));
            m_NumColumns = Math.Max(1, (int)(TextAreaSize.Width / m_CharSize.Width));
            scrollbar.LargeChange = m_NumLines;
            hscrollbar.LargeChange = m_NumColumns;
        }

        private void CalculateCharSize()
        {
            using (Graphics g = CreateGraphics())
            {
                SizeF sz = g.MeasureString("aaaaaaaaaaaaaaaxaa", Font);
                m_CharSize = new SizeF(sz.Width / 18.0f, Font.GetHeight());
                m_TabCharSize = new SizeF(m_CharSize.Width * 8.0f, m_CharSize.Height);
            }
        }

        private void SetBackColorFormat()
        {
        }

        private void ResetFormatLayers()
        {
            m_FormatLayers.Clear();
            FormattedTextLayer layer = new FormattedTextLayer();
            layer.Spans.Add(new FormattedTextSpan() { Format = new TextFormat(2, 3) });
            m_FormatLayers.Add(layer);
            m_FormatLayers.Add(new FormattedTextLayer());
            m_FormatLayers.Add(new FormattedTextLayer());
            m_FormatLayers.Add(new FormattedTextLayer());
            m_FormatLayers.Add(new FormattedTextLayer());
        }

        private Size TextAreaSize
        {
            get { return new Size(ClientSize.Width - m_TextMarginFirst - m_TextMarginSecond - scrollbar.Width + 1, ClientSize.Height - hscrollbar.Height + 1); }
        }

        private FormattedTextDocument m_FormattedDocument;
        
        public FormattedTextDocument FormattedDocument
        {
            get
            {
                return m_FormattedDocument;
            }
            set
            {
                m_FormattedDocument = value;
                ResetFormatLayers();
                if (value != null)
                {
                    m_Document = value.Document;
                    scrollbar.Maximum = Math.Max(0, m_Document.LineCount - 1);
                    hscrollbar.Maximum = Math.Max(0, m_Document.GetLongestLineLength());
                    m_FormatLayers[3] = m_FormattedDocument.ColorLayer;
                    m_FormatLayers[4] = m_FormattedDocument.BlockLayer;
                }
                else
                {
                    m_Document = new TextDocument("");
                    scrollbar.Maximum = 0;
                    hscrollbar.Maximum = 0;
                }
                ScrollTo(0);
            }
        }

        public int CurrentLine
        {
            get { return m_CurrentLine; }
        }

        private int m_TextMarginFirst;

        [Browsable(true)]
        public int TextMarginFirst
        {
            get { return m_TextMarginFirst; }
            set { m_TextMarginFirst = value; Invalidate(); }
        }

        private int m_TextMarginSecond;

        [Browsable(true)]
        public int TextMarginSecond
        {
            get { return m_TextMarginSecond; }
            set { m_TextMarginSecond = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(BackColor), e.ClipRectangle);

            Rectangle firstMargin = new Rectangle(0, 0, m_TextMarginFirst, TextAreaSize.Height);
            firstMargin.Intersect(e.ClipRectangle);
            e.Graphics.FillRectangle(SystemBrushes.Control, firstMargin);

            int firstLine = (int)(e.ClipRectangle.Top / m_CharSize.Height) + m_CurrentLine;
            int lastLine = (int)Math.Ceiling(e.ClipRectangle.Bottom / m_CharSize.Height) + m_CurrentLine;
            int numLines = lastLine - firstLine + 1;
            int maxx = TextAreaSize.Width + m_TextMarginFirst + m_TextMarginSecond;

            int start = m_Document.GetIndexOfLine(firstLine);
            int end = m_Document.GetIndexOfLine(firstLine + numLines);
            TextFormat[] f = GetTextFormatting(start, end);

            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            PointF pt = new PointF(m_TextMarginFirst + m_TextMarginSecond, (firstLine - m_CurrentLine) * m_CharSize.Height);
            for (int i = 0, line = firstLine, pos = 0; i < numLines; i++, line++)
            {
                string s = m_Document.GetSpanForLine(line).Text;
                SizeF[] charSize = new SizeF[s.Length];
                for (int j = m_CurrentColumn; j < s.Length - 1; j++)
                    charSize[j] = m_CharSize; // s[j] == '\t' ? m_TabCharSize : 
                if (s.Length > 0)
                    charSize[s.Length - 1] = new SizeF(ClientSize.Width - pt.X, m_CharSize.Height);

                // Color margin
                int margin = 0;
                for (int j = 0; j < s.Length; j++)
                    if (f[pos + j].Margin > 0)
                    {
                        margin = f[pos + j].Margin;
                        break;
                    }
                if (margin > 0)
                    e.Graphics.FillRectangle(m_Formats[margin], new RectangleF(0.0f, pt.Y, m_TextMarginFirst, m_CharSize.Height));

                PointF p = pt;
                for (int j = m_CurrentColumn; j < s.Length; j++)
                {
                    e.Graphics.FillRectangle(m_Formats[f[pos + j].Back], new RectangleF(p, charSize[j]));
                    p.X += charSize[j].Width;
                    if ((int)p.X >= maxx)
                        break;
                }
                if (m_CurrentColumn >= s.Length && s.Length > 0)
                {
                    e.Graphics.FillRectangle(m_Formats[f[pos + s.Length - 1].Back], new RectangleF(pt, charSize[s.Length - 1]));
                }

                p = pt;
                p.X -= 1.0f;
                for (int j = m_CurrentColumn; j < s.Length; j++)
                {
                    int fmt = f[pos + j].Front;
                    e.Graphics.DrawString(s[j].ToString(), fmt == 23 ? m_BoldFont : Font, m_Formats[fmt], p);
                    p.X += charSize[j].Width;
                    if ((int)p.X >= maxx)
                        break;
                }

                pt.Y += m_CharSize.Height;
                pos += s.Length;
            }

            if (m_InScrollAnchorMode)
            {
                Cursor c = Cursors.NoMove2D;
                c.Draw(e.Graphics, new Rectangle(m_ScrollAnchor.X - c.HotSpot.X, m_ScrollAnchor.Y - c.HotSpot.Y, 0, 0));
            }

            Rectangle bottomRightCorner = new Rectangle(ClientSize.Width - scrollbar.Width, ClientSize.Height - hscrollbar.Height, scrollbar.Width, hscrollbar.Height);
            bottomRightCorner.Intersect(e.ClipRectangle);
            e.Graphics.FillRectangle(SystemBrushes.Control, bottomRightCorner);

            base.OnPaint(e);
        }

        public void InvalidateLines(int startLine, int endLine)
        {
            Invalidate(Rectangle.FromLTRB(0, (int)((startLine - m_CurrentLine) * m_CharSize.Height),
                ClientSize.Width, (int)Math.Ceiling((endLine + 1 - m_CurrentLine) * m_CharSize.Height)));
        }

        private TextFormat[] GetTextFormatting(int start, int end)
        {
            TextFormat[] result = new TextFormat[end - start];

            List<FormattedTextLayerChunk> chunks = m_FormatLayers.Select(layer => new FormattedTextLayerChunk(layer, start, end)).ToList();

            for (int i = 0; i < end - start; i++)
            {
                var formats = chunks.Select(chunk => chunk.GetFormatAt(start + i)).ToList();
                int front = formats.Select(f => f.Front).SkipWhile(v => v == 0).DefaultIfEmpty((byte)1).First();
                int back = formats.Select(f => f.Back).SkipWhile(v => v == 0).DefaultIfEmpty((byte)2).First();
                int margin = formats.Select(f => f.Margin).SkipWhile(v => v == 0).DefaultIfEmpty((byte)0).First();
                result[i] = new TextFormat(front, back, margin);
            }

            return result;
        }

        private void scrollbar_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                m_CurrentLine = e.NewValue;
                Invalidate();
            }
        }

        private void hscrollbar_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                m_CurrentColumn = e.NewValue;
                Invalidate();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            ScrollTo(m_CurrentLine - Math.Sign(e.Delta) * 3);
        }

        public void ScrollTo(int line)
        {
            m_CurrentLine = Math.Max(0, Math.Min(line, m_Document.LineCount - m_NumLines));
            scrollbar.Value = m_CurrentLine;
            Invalidate();
        }

        public void ScrollToCenter(SimpleTextSpan span)
        {
            ScrollToCenter(span.Start, span.End);
        }

        public void ScrollToCenter(int start, int end)
        {
            int firstLine = m_Document.GetLine(start);
            int lastLine = m_Document.GetLine(end);
            int lineCount = lastLine - firstLine + 1;
            m_CurrentColumn = 0;
            ScrollTo(firstLine - Math.Max((m_NumLines - lineCount) / 2, 2));
        }

        public int GetTextIndex(PointF p)
        {
            int line = m_CurrentLine + (int)(p.Y / m_CharSize.Height);
            int col = m_CurrentColumn + (int)Math.Round((p.X - m_TextMarginFirst - m_TextMarginSecond) / m_CharSize.Width);
            return Math.Min(m_Document.GetIndexOfLine(line) + col, m_Document.GetIndexOfLine(line + 1));
        }

        public int GetTextCharIndex(PointF p)
        {
            int line = m_CurrentLine + (int)(p.Y / m_CharSize.Height);
            int col = m_CurrentColumn + (int)((p.X - m_TextMarginFirst - m_TextMarginSecond) / m_CharSize.Width);
            return Math.Min(m_Document.GetIndexOfLine(line) + col, m_Document.GetIndexOfLine(line + 1));
        }

        public string SelectedText
        {
            get { return m_Document.FullText.Substring(m_FormatLayers[0].Spans[0].Start, m_FormatLayers[0].Spans[0].Length); }
        }

        public FormattedEntity SelectedEntity
        {
            get { return m_SelectedEntity; }
        }

        private void InvalidateSelection()
        {
            int start = m_FormatLayers[0].Spans[0].Start;
            int end = m_FormatLayers[0].Spans[0].End;
            if (m_FormatLayers[1].Spans.Count >= 1)
            {
                start = Math.Min(start, m_FormatLayers[1].Spans.First().Start);
                end = Math.Max(end, m_FormatLayers[1].Spans.Last().End);
            }
            InvalidateLines(m_Document.GetLine(start), m_Document.GetLine(end));
        }

        public void SelectText(int a, int b)
        {
            InvalidateSelection();

            m_FormatLayers[0].Spans[0].Start = Math.Min(a, b);
            m_FormatLayers[0].Spans[0].End = Math.Max(a, b);

            FormattedTextSpan span = m_FormatLayers[4].GetRange(Math.Min(a, b), Math.Max(a, b)).SkipWhile(s => s.Length == 0).FirstOrDefault();
            SelectEntity(span != null ? span.Entity : null);

            InvalidateSelection();
        }

        public void UnselectText()
        {
            InvalidateSelection();

            m_FormatLayers[0].Spans[0].Start = 0;
            m_FormatLayers[0].Spans[0].End = 0;
        }

        public void SelectAndScrollToEntityAt(TextSpan ts)
        {
            FormattedTextSpan span = m_FormatLayers[4].GetRange(ts.Start, ts.End).FirstOrDefault();
            if (span != null)
            {
                SelectEntity(span.Entity);
                ScrollToCenter(span.Start, span.End);
            }
            else
            {
                SelectPseudoEntity(ts);
                ScrollToCenter(ts.Start, ts.End);
            }
        }

        public void SelectAndScrollToEntity(FormattedEntity e)
        {
            SelectEntity(e);
            ScrollToCenter(e.TotalSpan);
        }

        public void SelectAndScrollToPseudoEntity(TextSpan textSpan)
        {
            SelectPseudoEntity(textSpan);
            ScrollToCenter(textSpan.Start, textSpan.End);
        }

        public void Unselect()
        {
            InvalidateSelection();
            UnselectText();
            SelectEntity(null);
        }

        private void SelectEntity(FormattedEntity e)
        {
            if (e != null)
            {
                m_FormatLayers[1].Spans = e.Spans.Select(span => new FormattedTextSpan() { Start = span.Start, End = span.End, Format = new TextFormat(0, 6, 8) }).ToList();
                var totalSpan = e.TotalSpan;
                m_FormatLayers[2].Spans.Clear();
                m_FormatLayers[2].Spans.Add(new FormattedTextSpan() { Start = e.TotalSpan.Start, End = e.TotalSpan.End, Format = new TextFormat(0, 0, 9) });
            }
            else
            {
                m_FormatLayers[1].Spans.Clear();
                m_FormatLayers[2].Spans.Clear();
            }
            m_SelectedEntity = e;
        }

        private void SelectPseudoEntity(TextSpan ts)
        {
            m_FormatLayers[1].Spans.Clear();
            m_FormatLayers[1].Spans.Add(new FormattedTextSpan() { Start = ts.Start, End = ts.End, Format = new TextFormat(0, 6, 8) });
            m_FormatLayers[2].Spans.Clear();
        }

        private void UnselectOrKeep(int pos)
        {
            FormattedTextSpan span = m_FormatLayers[4].GetRange(pos, pos).SkipWhile(s => s.Length == 0).FirstOrDefault();
            if (span == null || span.Entity != m_SelectedEntity)
            {
                Unselect();
            }
        }

        private Cursor m_PrevCursor;
        private bool m_InScrollAnchorMode = false;
        private bool m_InScrollAnchorClickToReleaseMode;
        private bool m_ScrollAnchorHasScrolled;
        private Point m_ScrollAnchor;
        private float m_ScrollAnchorSpeed;
        private float m_ScrollAnchorPos;

        private bool m_InSelectionMode = false;
        private int m_SelectionStartIndex;

        private bool m_AfterDoubleClickMode = false;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (m_InScrollAnchorClickToReleaseMode)
            {
                CancelScrollAnchorMode();
                return;
            }

            if ((e.Button & MouseButtons.Middle) == MouseButtons.Middle)
            {
                m_InScrollAnchorMode = true;
                m_InScrollAnchorClickToReleaseMode = false;
                m_ScrollAnchor = e.Location;
                m_PrevCursor = Cursor;
                Cursor = Cursors.NoMove2D;
                m_ScrollAnchorPos = m_CurrentLine;
                m_ScrollAnchorSpeed = 0;
                tmrScrollAnchor.Enabled = true;
                m_ScrollAnchorHasScrolled = false;
                Invalidate();
            }
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                m_InSelectionMode = true;
                m_SelectionStartIndex = GetTextIndex(e.Location);
                SelectText(m_SelectionStartIndex, m_SelectionStartIndex);
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (m_InScrollAnchorMode)
            {
                m_ScrollAnchorSpeed = (e.Y - m_ScrollAnchor.Y) / 25.0f;
                if (Math.Abs(m_ScrollAnchorSpeed) > 12.0f)
                    m_ScrollAnchorSpeed *= 5.0f;
                else if (Math.Abs(m_ScrollAnchorSpeed) > 6.0f)
                    m_ScrollAnchorSpeed *= 3.0f;
                else if (Math.Abs(m_ScrollAnchorSpeed) < 1.0f)
                    m_ScrollAnchorSpeed = 0;
                int dir = Math.Sign(m_ScrollAnchorSpeed);
                if (dir < 0)
                    Cursor = Cursors.PanNorth;
                else if (dir > 0)
                    Cursor = Cursors.PanSouth;
                else Cursor = Cursors.NoMove2D;
            }
            if (m_InSelectionMode)
            {
                if (e.Location.Y < 0)
                    ScrollTo(m_CurrentLine - 1);
                else if (e.Location.Y > ClientSize.Height)
                    ScrollTo(m_CurrentLine + 1);
                int index = GetTextIndex(e.Location);
                SelectText(m_SelectionStartIndex, index);
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (m_InScrollAnchorMode)
            {
                if (m_ScrollAnchorHasScrolled || m_InScrollAnchorClickToReleaseMode)
                {
                    CancelScrollAnchorMode();
                }
                else
                {
                    m_InScrollAnchorClickToReleaseMode = true;
                }
            }
            if (m_InSelectionMode)
            {
                if (!m_AfterDoubleClickMode)
                {
                    int index = GetTextIndex(e.Location);
                    SelectText(m_SelectionStartIndex, index);
                }
                else
                {
                    m_AfterDoubleClickMode = false;
                }
                m_InSelectionMode = false;
            }
            if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
            {
                UnselectOrKeep(GetTextIndex(e.Location));
                if (m_SelectedEntity != null)
                    contextMenuStrip1.Show(this, e.Location);
            }
            base.OnMouseUp(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            TextSpan span = m_Document.GetIdentifierAt(GetTextCharIndex(e.Location));
            if (span != null) SelectText(span.Start, span.End);
            m_AfterDoubleClickMode = true;
            base.OnMouseDoubleClick(e);
        }

        private void tmrScrollAnchor_Tick(object sender, EventArgs e)
        {
            int last = (int)Math.Round(m_ScrollAnchorPos);
            m_ScrollAnchorPos = Math.Max(0, Math.Min(m_ScrollAnchorPos + m_ScrollAnchorSpeed, m_Document.LineCount - m_NumLines));
            int current = (int)Math.Round(m_ScrollAnchorPos);
            if (last != current)
            {
                ScrollTo(current);
                m_ScrollAnchorHasScrolled = true;
            }
        }

        private void CancelScrollAnchorMode()
        {
            m_InScrollAnchorMode = false;
            m_InScrollAnchorClickToReleaseMode = false;
            Cursor = m_PrevCursor;
            tmrScrollAnchor.Enabled = false;
            Invalidate();
        }

        private void mnuWhereCalled_Click(object sender, EventArgs e)
        {
            if (HighlightEntityUsage != null && m_SelectedEntity != null)
                HighlightEntityUsage(this, new CodeViewEventArgs(m_SelectedEntity));
        }

        private void btnFocusDummy_KeyDown(object sender, KeyEventArgs e)
        {
            // For some reason this.OnKeyDown() does not receive keystrokes... (used to work actually, for some reason doesn't work anymore)
            // So instead we use btnFocusDummy, which will get the focus when the CodeView gets the focus.
            if (e.KeyCode == Keys.Up)
                ScrollTo(m_CurrentLine - 1);
            else if (e.KeyCode == Keys.Down)
                ScrollTo(m_CurrentLine + 1);
            else if (e.KeyCode == Keys.PageUp)
                ScrollTo(m_CurrentLine - m_NumLines);
            else if (e.KeyCode == Keys.PageDown)
                ScrollTo(m_CurrentLine + m_NumLines);
            else if (e.KeyCode == Keys.Home && e.Control)
                ScrollTo(0);
            else if (e.KeyCode == Keys.End && e.Control)
                ScrollTo(int.MaxValue);
            else if ((e.KeyCode == Keys.C && e.Control) || (e.KeyCode == Keys.Insert && e.Control))
                Clipboard.SetText(SelectedText);
            else if (e.KeyCode == Keys.Escape)
                Unselect();
        }
    }

    public class FocusDummyButton : Button
    {
        protected override bool IsInputKey(Keys key)
        {
            return base.IsInputKey(key) || key == Keys.Left || key == Keys.Right || key == Keys.Up || key == Keys.Down;
        }
    }

    public class CodeViewEventArgs : EventArgs
    {
        public FormattedEntity Entity { get; private set; }

        public CodeViewEventArgs(FormattedEntity e)
        {
            Entity = e;
        }
    }

    public struct TextFormat
    {
        public int Front;
        public int Back;
        public int Margin;
        public static TextFormat None = new TextFormat(0, 0);
        public TextFormat(int front, int back) : this(front, back, 0) { }
        public TextFormat(int front, int back, int margin) { Front = front; Back = back; Margin = margin; }
    }

    public class FormattedTextLayer
    {
        public List<FormattedTextSpan> Spans { get; set; }

        public FormattedTextLayer()
        {
            Spans = new List<FormattedTextSpan>();
        }

        public IEnumerable<FormattedTextSpan> GetRange(int start, int end)
        {
            int index = Spans.BinarySearch(new FormattedTextSpan() { Start = start }, new FormattedTextSpanStartComparer());
            if (index < 0)
            {
                index = ~index;
                if (index > 0) index--;
                if (index < Spans.Count && (Spans[index].End < start || (Spans[index].End == start && Spans[index].Start < start)))
                    index++;
            }
            while (index < Spans.Count && Spans[index].Start <= end)
                yield return Spans[index++];
        }

        /*public FormattedTextSpan GetFirstSpanExactlyAt(int pos)
        {
            int index = Spans.BinarySearch(new FormattedTextSpan() { Start = pos, End = pos }, new FormattedTextSpanStartComparer());
            if (index < 0)
            {
                index = ~index;
                if (index == Spans.Count) return null;
                if (Spans[index].Start != pos) return null;
            }
            return Spans[index];
        }

        public FormattedTextSpan GetSpanAtOrAfter(int start)
        {
            int index = Spans.BinarySearch(new FormattedTextSpan() { Start = start, End = start }, new FormattedTextSpanInsideComparer());
            if (index >= 0)
                return Spans[index];
            else 
            {
                index = ~index;
                if (index < Spans.Count)
                    return Spans[index];
                else
                    return null;
            }
        }

        public FormattedTextSpan GetSpanAtOrBefore(int end)
        {
            int index = Spans.BinarySearch(new FormattedTextSpan() { Start = end, End = end }, new FormattedTextSpanInsideComparer());
            if (index >= 0)
                return Spans[index];
            else
            {
                index = (~index) - 1;
                if (index >= 0)
                    return Spans[index];
                else
                    return null;
            }
        }*/
    }

    public class FormattedTextLayerChunk
    {
        private List<FormattedTextSpan> m_Spans;
        private int m_CurrentIndex = 0;

        public FormattedTextLayerChunk(FormattedTextLayer layer, int start, int end)
        {
            m_Spans = layer.GetRange(start, end).ToList();
        }

        public TextFormat GetFormatAt(int pos)
        {
            while (m_CurrentIndex < m_Spans.Count && m_Spans[m_CurrentIndex].End <= pos)
                m_CurrentIndex++;
            if (m_CurrentIndex < m_Spans.Count && m_Spans[m_CurrentIndex].Start <= pos && pos < m_Spans[m_CurrentIndex].End)
                return m_Spans[m_CurrentIndex].Format;
            else
                return TextFormat.None;
        }
    }

    public class FormattedTextSpan
    {
        public int Start { set; get; }
        public int End { set; get; }
        public int Length { get { return End - Start; } }
        public TextFormat Format { set; get; }
        public FormattedEntity Entity { set; get; }
    }

    public class FormattedEntity
    {
        public List<FormattedTextSpan> Spans { get; set; }
        public List<FormattedEntity> Children { get; set; }
        public FormattedEntity()
        {
            Spans = new List<FormattedTextSpan>();
            Children = new List<FormattedEntity>();
        }

        public SimpleTextSpan TotalSpan
        {
            get
            {
                if (Spans.Count > 0)
                    return new SimpleTextSpan(Spans.First().Start, Spans.Last().End);
                else
                    return new SimpleTextSpan(Children.First().TotalSpan.Start, Children.Last().TotalSpan.End);
            }
        }
    }

    public class FormattedTextSpanStartComparer : IComparer<FormattedTextSpan>
    {
        public int Compare(FormattedTextSpan x, FormattedTextSpan y) { return x.Start.CompareTo(y.Start); }
    }

    public class FormattedTextSpanStrictOrderComparer : IComparer<FormattedTextSpan>
    {
        public int Compare(FormattedTextSpan x, FormattedTextSpan y)
        {
            int cmp = x.Start.CompareTo(y.Start);
            return cmp != 0 ? cmp : x.End.CompareTo(y.End);
        }
    }

    public class FormattedTextSpanInsideComparer : IComparer<FormattedTextSpan>
    {
        public int Compare(FormattedTextSpan x, FormattedTextSpan y)
        {
            int sign = -1;
            if (x.Start > y.Start)
            {
                FormattedTextSpan h = x; x = y; y = h;
                sign = 1;
            }
            if (y.End <= x.End) return 0;
            return sign;
        }
    }

    public class FormattedTextDocument
    {
        public TextDocument Document { get; private set; }
        public FormattedTextLayer ColorLayer { get; private set; }
        public FormattedTextLayer BlockLayer { get; private set; }

        public FormattedTextDocument(TextDocument doc)
        {
            Document = doc;
            ColorLayer = new FormattedTextLayer();
            BlockLayer = new FormattedTextLayer();
        }

        protected void NudgeBlocks()
        {
            for (int i = 0; i < BlockLayer.Spans.Count; i++)
            {
                FormattedTextSpan current = BlockLayer.Spans[i];
                TextSpan firstLine = Document.GetSpanForLine(Document.GetLine(current.Start));
                if (Document.FullText.Substring(firstLine.Start, current.Start - firstLine.Start).Trim() == "")
                {
                    current.Start = firstLine.Start;
                    if (i > 0) BlockLayer.Spans[i - 1].End = Math.Min(BlockLayer.Spans[i - 1].End, current.Start);
                }
                TextSpan lastLine = Document.GetSpanForLine(Document.GetLine(current.End));
                if (Document.FullText.Substring(current.End, lastLine.End - current.End).Trim() == "")
                {
                    current.End = lastLine.End;
                    if (i < BlockLayer.Spans.Count - 1) BlockLayer.Spans[i + 1].Start = Math.Max(BlockLayer.Spans[i + 1].Start, current.End);
                }
            }
        }
    }

    public class FormattedJavaScriptDocument : FormattedTextDocument
    {
        private Dictionary<Firefox.JavaScriptBlock, FormattedEntity> m_BlockToEntity = new Dictionary<Firefox.JavaScriptBlock, FormattedEntity>();
        private Dictionary<FormattedEntity, Firefox.JavaScriptBlock> m_EntityToBlock = new Dictionary<FormattedEntity, Firefox.JavaScriptBlock>();

        public FormattedJavaScriptDocument(Firefox.HtmlDocument htdoc)
            : base(htdoc.Original)
        {
            AddJavaScriptSnippets(htdoc.JavaScriptSnippets);
            FinishBlockLayer();
        }

        public FormattedJavaScriptDocument(Firefox.JavaScriptDocument jsdoc)
            : base(jsdoc.Original)
        {
            AddJavaScriptSnippet(jsdoc);
            FinishBlockLayer();
        }

        private void AddJavaScriptSnippets(IEnumerable<Firefox.JavaScriptDocument> jsdocs)
        {
            int last = 0;
            foreach (Firefox.JavaScriptDocument jsdoc in jsdocs)
            {
                ColorLayer.Spans.Add(new FormattedTextSpan() { Start = last, End = jsdoc.OriginalSpan.Start, Format = new TextFormat(0, 30) });
                AddJavaScriptSnippet(jsdoc);
                last = jsdoc.OriginalSpan.End;
            }
            ColorLayer.Spans.Add(new FormattedTextSpan() { Start = last, End = Document.FullText.Length, Format = new TextFormat(0, 30) });
        }

        private void AddJavaScriptSnippet(Firefox.JavaScriptDocument jsdoc)
        {
            ColorLayer.Spans.AddRange(
                jsdoc.Tokens.Select(token =>
                    new FormattedTextSpan() { Start = token.Location.Start, End = token.Location.End, Format = new TextFormat((byte)(token.Type + 10), 0) }));
            BuildBlockLayer(jsdoc.Block, jsdoc.Block.End > 0, new List<SimpleTextSpan>(), new FormattedEntity(), false);
        }

        private void BuildBlockLayer(Firefox.JavaScriptBlock block, bool buildThis, List<SimpleTextSpan> spans, FormattedEntity parent, bool color)
        {
            if (buildThis)
            {
                FormattedEntity entity = new FormattedEntity();
                parent.Children.Add(entity);
                m_BlockToEntity.Add(block, entity);
                m_EntityToBlock.Add(entity, block);

                List<SimpleTextSpan> innerSpans = new List<SimpleTextSpan>();
                foreach (Firefox.JavaScriptBlock b in block.SubBlocks)
                    BuildBlockLayer(b, b.IsFunction(), innerSpans, entity, !color);

                var outerSpans = block.GetTextLocation().InvertBits(innerSpans).ToList();
                var formatSpans = outerSpans.Select(loc => new FormattedTextSpan() { Start = loc.Start, End = loc.End, Format = new TextFormat(0, (byte)(4 + (color ? 1 : 0))), Entity = entity }).ToList();
                BlockLayer.Spans.AddRange(formatSpans);
                entity.Spans.AddRange(formatSpans);

                spans.Add(block.GetTextLocation());
            }
            else
            {
                foreach (Firefox.JavaScriptBlock b in block.SubBlocks)
                    BuildBlockLayer(b, b.IsFunction(), spans, parent, color);
            }
        }

        private void FinishBlockLayer()
        {
            BlockLayer.Spans.Sort(new FormattedTextSpanStrictOrderComparer());
            NudgeBlocks();
        }

        /*private void BuildBlockLayer(Firefox.JavaScriptDocument jsdoc)
        {
            int s = 0;
            BuildBlockLayer(jsdoc.Block, 0, 0, null, ref s);
            NudgeBlocks(jsdoc);
        }

        private void BuildBlockLayer(Firefox.JavaScriptBlock b, int color, int prevColor, FormattedEntity prevEntity, ref int prevStart)
        {
            FormattedEntity entity = null;
            var loc = b.GetTextLocation();
            if (color != prevColor && prevColor > 0)
            {
                var span = new FormattedTextSpan() { Start = prevStart, End = loc.Start, Format = new TextFormat(0, (short)(prevColor + 7)), Entity = prevEntity };
                BlockLayer.Spans.Add(span);
                prevEntity.Spans.Add(span);
            }
            if (color != prevColor && color > 0)
            {
                prevStart = loc.Start;
                entity = new FormattedEntity();
            }

            foreach (Firefox.JavaScriptBlock block in b.SubBlocks)
                BuildBlockLayer(block, block.IsFunction() ? (color + 1 > 2 ? 1 + (color % 2) : color + 1) : color, color, entity, ref prevStart);

            if (color != prevColor && color > 0)
            {
                var span = new FormattedTextSpan() { Start = prevStart, End = loc.End, Format = new TextFormat(0, (short)(color + 7)), Entity = entity };
                BlockLayer.Spans.Add(span);
                entity.Spans.Add(span);
            }
            if (color != prevColor && prevColor > 0)
                prevStart = loc.End;
        }*/

        public FormattedEntity BlockToEntity(Firefox.JavaScriptBlock block)
        {
            return m_BlockToEntity[block];
        }

        public Firefox.JavaScriptBlock EntityToBlock(FormattedEntity e)
        {
            return m_EntityToBlock[e];
        }
    }

    public class FormattedJspDocument : FormattedTextDocument
    {
        private Java.JspDocument m_JspDoc;

        public FormattedJspDocument(Java.JspDocument doc)
            : base(doc.Original)
        {
            m_JspDoc = doc;

            ColorLayer.Spans.AddRange(
                doc.Tokens.Where(token => token.Type == Java.JspTokenType.Text || token.Type == Java.JspTokenType.TemplateComment || token.Type == Java.JspTokenType.TemplateOtherColored).Select(token =>
                    new FormattedTextSpan() { Start = token.Location.Start, End = token.Location.End, Format = new TextFormat((byte)(token.Type + 40), token.Type == Java.JspTokenType.Text ? 30 : 0) }));

            AddJavaSnippets(doc.JavaSnippets);
            ColorLayer.Spans.Sort(new Comparison<FormattedTextSpan>((a, b) => a.Start.CompareTo(b.Start)));

            //var span = new FormattedTextSpan() { Start = 0, End = doc.Original.FullText.Length, Format = new TextFormat(0, 4), Entity = e };
            //e.Spans.Add(span);
            //BlockLayer.Spans.Add(span);*/

            var e = new FormattedEntity();
            var spans = doc.Tokens.Where(token => token.Type != Java.JspTokenType.Text).Select(token =>
                new FormattedTextSpan() { Start = token.Location.Start, End = token.Location.End, Format = new TextFormat(0, 4), Entity = e });

            e.Spans.AddRange(spans);
            BlockLayer.Spans.AddRange(spans);
        }

        private void AddJavaSnippets(IEnumerable<Java.JavaDocument> jdocs)
        {
            foreach (Java.JavaDocument jdoc in jdocs)
                AddJavaSnippet(jdoc);
        }

        private void AddJavaSnippet(Java.JavaDocument jdoc)
        {
            ColorLayer.Spans.AddRange(
                jdoc.Tokens.Select(token =>
                    new FormattedTextSpan() { Start = token.Location.Start, End = token.Location.End, Format = new TextFormat((byte)(token.Type + 20), 0) }));
        }

        public Java.JspDocument EntityToDocument(FormattedEntity e)
        {
            return m_JspDoc;
        }
    }

    public class FormattedJavaDocument : FormattedTextDocument
    {
        private Dictionary<FormattedEntity, Java.JavaMethodBlock> m_EntityToBlock = new Dictionary<FormattedEntity, Java.JavaMethodBlock>();

        public FormattedJavaDocument(Java.JavaDocument jdoc)
            : base(jdoc.Original)
        {
            ColorLayer.Spans.AddRange(
                jdoc.Tokens.Select(token =>
                    new FormattedTextSpan() { Start = token.Location.Start, End = token.Location.End, Format = new TextFormat((byte)(token.Type + 20), 0) }));
            BlockLayer.Spans.AddRange(
                jdoc.Block.MethodBlocks.Select(block =>
                    {
                        var e = new FormattedEntity();
                        var span = new FormattedTextSpan() { Start = block.Location.Start, End = block.Location.End, Format = new TextFormat(0, 4), Entity = e };
                        e.Spans.Add(span);
                        m_EntityToBlock.Add(e, block);
                        return span;
                    }));
            NudgeBlocks();
        }

        public Java.JavaMethodBlock EntityToBlock(FormattedEntity e)
        {
            return m_EntityToBlock[e];
        }
    }
}

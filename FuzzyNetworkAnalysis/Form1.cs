using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.Json;
using System.IO;
using System.Linq;
using FuzzyNetworkAnalysis.FuzzyMath;

namespace FuzzyNetworkAnalysis
{
    public partial class Form1 : Form
    {
        List<FuzzyWork> works = new List<FuzzyWork>();
        bool valueChange = false;
        public Form1()
        {
            InitializeComponent();
        }
        #region Ввод данных
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (valueChange || e.RowIndex == -1)
                return;
            string temp = dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString();
            valueChange = true;

            if (works[e.RowIndex].number == null)
                works[e.RowIndex].number = new FuzzyNumber();
            switch (e.ColumnIndex)
            {
                case 0:
                    if(int.TryParse(temp,out int start))
                    {
                        works[e.RowIndex].Start = start;
                    }
                    else
                    {
                        dataGridView1[e.ColumnIndex, e.RowIndex].Value = works[e.RowIndex]?.Start?.ToString() ?? "";
                    }
                    break;
                case 1:
                    if (int.TryParse(temp, out int end))
                    {
                        works[e.RowIndex].End = end;
                    }
                    else
                    {
                        dataGridView1[e.ColumnIndex, e.RowIndex].Value = works[e.RowIndex]?.End?.ToString() ?? "";
                    }
                    break;
                case 2:
                    if (double.TryParse(temp, out double p1))
                    {
                        works[e.RowIndex].number.p1 = p1;
                    }
                    else
                    {
                        dataGridView1[e.ColumnIndex, e.RowIndex].Value = works[e.RowIndex]?.number?.p1?.ToString() ?? "";
                    }
                    break;
                case 3:
                    if (double.TryParse(temp, out double m1))
                    {
                        works[e.RowIndex].number.m1 = m1;
                    }
                    else
                    {
                        dataGridView1[e.ColumnIndex, e.RowIndex].Value = works[e.RowIndex]?.number?.m1?.ToString() ?? "";
                    }
                    break;
                case 4:
                    if (double.TryParse(temp, out double m2))
                    {
                        works[e.RowIndex].number.m2 = m2;
                    }
                    else
                    {
                        dataGridView1[e.ColumnIndex, e.RowIndex].Value = works[e.RowIndex]?.number?.m2?.ToString() ?? "";
                    }
                    break;
                case 5:
                    if (double.TryParse(temp, out double p2))
                    {
                        works[e.RowIndex].number.p2 = p2;
                    }
                    else
                    {
                        dataGridView1[e.ColumnIndex, e.RowIndex].Value = works[e.RowIndex]?.number?.p2?.ToString() ?? "";
                    }
                    break;
            }
            valueChange = false;
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (!valueChange)
                works.Add(new FuzzyWork());
        }

        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            if (works.Count != 0 && !valueChange)
                works.RemoveAt(e.RowIndex);
        }

        private async void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (FileStream fs = new FileStream(openFileDialog1.FileName, FileMode.OpenOrCreate))
                    {
                        works = (await JsonSerializer.DeserializeAsync<FuzzyWork[]>(fs)).ToList();
                    }
                    valueChange = true;
                    dataGridView1.Rows.Clear();
                    dataGridView1.Rows.Add(works.Count);
                    for (int i = 0; i < works.Count; i++)
                    {
                        dataGridView1.Rows[i].Cells[0].Value = works[i]?.Start?.ToString() ?? "";
                        dataGridView1.Rows[i].Cells[1].Value = works[i]?.End?.ToString() ?? "";
                        dataGridView1.Rows[i].Cells[2].Value = works[i]?.number?.p1?.ToString() ?? "";
                        dataGridView1.Rows[i].Cells[3].Value = works[i]?.number?.m1?.ToString() ?? "";
                        dataGridView1.Rows[i].Cells[4].Value = works[i]?.number?.m2?.ToString() ?? "";
                        dataGridView1.Rows[i].Cells[5].Value = works[i]?.number?.p2?.ToString() ?? "";
                    }
                    valueChange = false;
                }
                catch { }
            }
        }

        private async void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create))
                {
                    await JsonSerializer.SerializeAsync<FuzzyWork[]>(fs, works.ToArray());
                }
            }
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            if (works.Count == 0)
                return;
            var worksOrdered = works.OrderBy(x => x.Start).ThenBy(x => x.End).ToList();
            bool failed = false;
            foreach (var work in worksOrdered)
                failed = failed || work == null || work.Start == null || work.End == null || work.number.p1 == null || work.number.m1 == null || work.number.m2 == null || work.number.p2 == null;
            if (failed)
            {
                label2.Text = "Вычислить время выполнения проекта\n\nВ таблице не должно быть пустых ячеек";
                return;
            }
            label2.Text = "Вычислить время выполнения проекта";

            #region Алгоритм

            #region Формироввание альфа-срезов
            List<List<FuzzyInterval>> alphasrezy = new List<List<FuzzyInterval>>();
            for (int i = 0; i < 6; i++)
            {
                double alpha = i * 0.2;
                List<FuzzyInterval> alphasrez = new List<FuzzyInterval>();
                for (int j = 0; j < worksOrdered.Count; j++)
                {
                    alphasrez.Add(new FuzzyInterval(
                        (worksOrdered[j].number.p1 ?? 0) + ((worksOrdered[j].number.m1 ?? 0) - (worksOrdered[j].number.p1 ?? 0)) * alpha,
                        (worksOrdered[j].number.p2 ?? 0) - ((worksOrdered[j].number.p2 ?? 0) - (worksOrdered[j].number.m2 ?? 0)) * alpha
                    ));
                }
                alphasrezy.Add(alphasrez);
            }
            #endregion

            List<List<WorkDeadline>> earlyDeadlineAlpha = new List<List<WorkDeadline>>();
            List<List<WorkDeadline>> lateDeadlineAlpha = new List<List<WorkDeadline>>();
            List<List<int>> CriticalTrack = new List<List<int>>();
            
            for (int i = 0; i < 6; i++) {
                #region Ранний срок
                List<WorkDeadline> earlyDeadlines = new List<WorkDeadline>();
                earlyDeadlines.Add(new WorkDeadline(worksOrdered[0].Start ?? 0, new FuzzyInterval()));
                List<List<int>> track = new List<List<int>>() { new List<int>() { worksOrdered[0].Start ?? 0 } };
                int iterator = 0;

                while (iterator < earlyDeadlines.Count)
                {
                    for (int j = 0; j < worksOrdered.Count; j++)
                    {
                        if (worksOrdered[j].Start == earlyDeadlines[iterator].work_id)
                        {
                            int index = earlyDeadlines.FindIndex(d => d.work_id == worksOrdered[j].End);
                            if (index == -1)
                            {
                                earlyDeadlines.Add(new WorkDeadline(worksOrdered[j].End ?? 0, new FuzzyInterval()));
                                index = earlyDeadlines.Count - 1;
                                track.Add(new List<int>());
                            }
                            earlyDeadlines[index].interval.L = Math.Max(earlyDeadlines[index].interval.L, earlyDeadlines[iterator].interval.L + alphasrezy[i][j].L);
                            if (earlyDeadlines[iterator].interval.R + alphasrezy[i][j].R > earlyDeadlines[index].interval.R)
                            {
                                earlyDeadlines[index].interval.R = earlyDeadlines[iterator].interval.R + alphasrezy[i][j].R;
                                track[index] = track[iterator].Union(new List<int>() { worksOrdered[j].End ?? 0 }).ToList();
                            }
                        }
                    }
                    iterator++;
                }

                int indexLast = earlyDeadlines.FindIndex(d1 => d1.work_id == earlyDeadlines.Max(d2 => d2.work_id));
                CriticalTrack.Add(track[indexLast]);
                var temp = earlyDeadlines[indexLast];
                earlyDeadlines.RemoveAt(indexLast);
                earlyDeadlines.Add(temp);
                earlyDeadlineAlpha.Add(earlyDeadlines);

                #endregion
                #region Поздний срок
                List<WorkDeadline> lateDeadlines = new List<WorkDeadline>();
                double max = earlyDeadlines.Last().interval.R;
                foreach(var deadline in earlyDeadlines)
                {
                    lateDeadlines.Add(new WorkDeadline(deadline.work_id, new FuzzyInterval(max, max)));
                }
                while (iterator > 0)
                {
                    iterator--;
                    for (int j = 0; j < worksOrdered.Count; j++)
                    {
                        if (worksOrdered[j].End == lateDeadlines[iterator].work_id)
                        {
                            int index = lateDeadlines.FindIndex(d => d.work_id == worksOrdered[j].Start);
                            lateDeadlines[index].interval.L = Math.Min(lateDeadlines[index].interval.L, lateDeadlines[iterator].interval.L - alphasrezy[i][j].R);
                            lateDeadlines[index].interval.R = Math.Min(lateDeadlines[index].interval.R, lateDeadlines[iterator].interval.R - alphasrezy[i][j].L);
                        }
                    }
                }
                lateDeadlineAlpha.Add(lateDeadlines);
                #endregion
            }

            #endregion
            #region Оценка риска
            double
                Tp1 = 0,
                Tm1 = 0,
                Tm2 = 0,
                Tp2 = 0;
            if(double.TryParse(textBox1.Text,out double T))
                Tp1 = T;
            if (double.TryParse(textBox2.Text, out T))
                Tm1 = T;
            if (double.TryParse(textBox3.Text, out T))
                Tm2 = T;
            if (double.TryParse(textBox4.Text, out T))
                Tp2 = T;

            double Sum = 0;
            if (0 < Tp1 && Tp1 <= Tm1 && Tm1 <= Tm2 && Tm2 <= Tp2)
            {
                List<FuzzyInterval> T_alphasrezy = new List<FuzzyInterval>();
                for (int i = 0; i < 6; i++)
                {
                    double alpha = 0.2 * i;
                    T_alphasrezy.Add(new FuzzyInterval(
                        Tp1 + (Tm1 - Tp1) * alpha,
                        Tp2 - (Tp2 - Tm2) * alpha
                        ));
                }

                for (int i = 0; i < 6; i++)
                {
                    /*
                    if (T < earlyDeadlineAlpha[i].Last().interval.L)
                        Sum += 0.2 * i;
                    else if (T_ <= earlyDeadlineAlpha[i].Last().interval.R)
                        Sum += 0.2 * i * (earlyDeadlineAlpha[i].Last().interval.R - T_) / (earlyDeadlineAlpha[i].Last().interval.R - earlyDeadlineAlpha[i].Last().interval.L) ?? 0;
                    */

                    if (T_alphasrezy[i].R < earlyDeadlineAlpha[i].Last().interval.L)
                        Sum += 0.2 * i;
                    else if (T_alphasrezy[i].R < earlyDeadlineAlpha[i].Last().interval.R)
                        Sum += 0.2 * i * (earlyDeadlineAlpha[i].Last().interval.R - T_alphasrezy[i].R) / (earlyDeadlineAlpha[i].Last().interval.R - earlyDeadlineAlpha[i].Last().interval.L);
                    else if (T_alphasrezy[i].L < earlyDeadlineAlpha[i].Last().interval.R)
                        Sum += 0.2 * i * (earlyDeadlineAlpha[i].Last().interval.R - T_alphasrezy[i].L) / (T_alphasrezy[i].R - Math.Max(earlyDeadlineAlpha[i].Last().interval.L, T_alphasrezy[i].L));
                }
            }

            Sum /= 3;
            #endregion
            #region Вывод результатов
            String criticalTrackString = "";
            foreach (var track in CriticalTrack.Last())
                criticalTrackString += track.ToString() + ", ";
            label2.Text = "Вычислить время выполнения проекта\n\n" +
                "Время выполнения проекта в виде нечеткого числа:\n" +
                "Критический путь:\n" +
                $"Оценка риска: {String.Format("{0:0.##}", 100 * Sum)}%";
            label3.Text = $"[{earlyDeadlineAlpha[0].Last().interval.L}, {earlyDeadlineAlpha[5].Last().interval.L}, {earlyDeadlineAlpha[5].Last().interval.R}, {earlyDeadlineAlpha[0].Last().interval.R}]\n" +
                criticalTrackString;
            String label5Text = "";
            for (int i = 0; i < 6; i++)
            {
                label5Text += $"Альфа-срез на уровне {String.Format("{0:0.#}", i * 0.2)}: [{earlyDeadlineAlpha[i].Last().interval.L}, {earlyDeadlineAlpha[i].Last().interval.R}]\n";
            }
            label5.Text = label5Text;
            #endregion
            #region plot
            foreach (var series in chart1.Series)
                series.Points.Clear();
            List<double> X1= new List<double>();
            for (int i = 0; i < 6; i++)
                X1.Add(earlyDeadlineAlpha[i].Last().interval.L);
            for (int i = 5; i >= 0; i--)
                X1.Add(earlyDeadlineAlpha[i].Last().interval.R);
            List<double> X2 = new List<double>();
            for (int i = 0; i < 6; i++)
                X2.Add(lateDeadlineAlpha[i][0].interval.R);
            for (int i = 5; i >= 0; i--)
                X2.Add(lateDeadlineAlpha[i][0].interval.L);
            List<double> Y = new List<double>() { 0, 0.2, 0.4, 0.6, 0.8, 1, 1, 0.8, 0.6, 0.4, 0.2, 0 };
            for(int i=0;i<12;i++)
                chart1.Series[0].Points.AddXY(X1[i], Y[i]);
            for (int i = 0; i < 12; i++)
                chart1.Series[1].Points.AddXY(X2[i], Y[i]);
            if (0 < Tp1 && Tp1 <= Tm1 && Tm1 <= Tm2 && Tm2 <= Tp2)
            {
                chart1.Series[2].Points.AddXY(Tp1, 0);
                chart1.Series[2].Points.AddXY(Tm1, 1);
                chart1.Series[2].Points.AddXY(Tm2, 1);
                chart1.Series[2].Points.AddXY(Tp2, 0);
            }
            #endregion

        }
    }
}

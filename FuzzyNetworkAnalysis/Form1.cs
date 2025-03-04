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
                failed = work == null || work.Start == null || work.End == null || work.number.p1 == null || work.number.m1 == null || work.number.m2 == null || work.number.p2 == null;
            if (failed)
            {
                label2.Text = "Вычислить время выполнения проекта\n\nВ таблице не должно быть пустых ячеек";
                return;
            }

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
            
            for (int i = 0; i < 6; i++) {
                #region Ранний срок
                List<WorkDeadline> earlyDeadlines = new List<WorkDeadline>();
                earlyDeadlines.Add(new WorkDeadline(worksOrdered[0].Start ?? 0, new FuzzyInterval()));
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
                            }
                            earlyDeadlines[index].interval.L = Math.Max(earlyDeadlines[index].interval.L, earlyDeadlines[iterator].interval.L + alphasrezy[i][j].L);
                            earlyDeadlines[index].interval.R = Math.Max(earlyDeadlines[index].interval.R, earlyDeadlines[iterator].interval.R + alphasrezy[i][j].R);
                        }
                    }
                    iterator++;
                }
                earlyDeadlineAlpha.Add(earlyDeadlines);
                #endregion
                #region Поздний срок
                List<WorkDeadline> lateDeadlines = new List<WorkDeadline>();
                double max = earlyDeadlines.Last().interval.R;
                for (int k = 0; k < earlyDeadlines.Count; k++)
                {
                    lateDeadlines.Add(new WorkDeadline(earlyDeadlines[k].work_id, new FuzzyInterval(max, max)));
                }
                while (iterator > 0)
                {
                    iterator--;
                    for (int j = 0; j < worksOrdered.Count; j++)
                    {
                        if (worksOrdered[j].End == lateDeadlines[iterator].work_id)
                        {
                            int index = lateDeadlines.FindIndex(d => d.work_id == worksOrdered[j].Start);
                            lateDeadlines[index].interval.L = Math.Min(lateDeadlines[index].interval.L, lateDeadlines[iterator].interval.L - alphasrezy[i][j].L);
                            lateDeadlines[index].interval.R = Math.Min(lateDeadlines[index].interval.R, lateDeadlines[iterator].interval.R - alphasrezy[i][j].R);
                        }
                    }
                }
                lateDeadlineAlpha.Add(lateDeadlines);
                #endregion
            }

            #endregion


            label2.Text = "Вычислить время выполнения проекта\n\nВремя выполнения проекта в виде нечеткого числа:\nКритический путь:";
            label3.Text = $"[{earlyDeadlineAlpha[0].Last().interval.L}, {earlyDeadlineAlpha[5].Last().interval.L}, {earlyDeadlineAlpha[5].Last().interval.R}, {earlyDeadlineAlpha[0].Last().interval.R}]\nlabel3";
        }
    }
}

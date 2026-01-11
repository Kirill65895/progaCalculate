using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mathos;
using Flee;
using Flee.PublicTypes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Linq.Expressions;
using Mathos.Parser;
using System.Runtime.Remoting.Channels;
using System.Diagnostics;

namespace Programma_2kyrs
{
    public partial class Form1 : Form
    {
        private ExpressionContext context = new ExpressionContext();
        // ХРАНЕНИЯ ТЕКУЩЕЙ ФУНКЦИИ
        private string currentFunction = "";
        private double lastRoot = 0;
        // Новые поля для метода золотого сечения
        private double lastMin = 0;
        private double lastMax = 0;
        private double lastZero = 0;
        private Panel graficPanel;
        // Интегралы
        private string lastIntegrationFunction = "";
        private double lastIntegrationA = 0;
        private double lastIntegrationB = 0;
        private string selectedIntegrationMethod = "Все методы";

        public Form1()
        {
            InitializeComponent();
            InitializeWindowComboBox();
            this.Text = "Калькулятор";
        }

        //*****************************************************************************| ТО ЧТО НЕ ВИДИТ ПОЛЬЗОВАТЕЛЬ И ЕМУ НЕ НАДО |***********************************************************************//

        // ПАНЕЛЬ ВЫБОРА ЗАДАНИЯ
        private void InitializeWindowComboBox()
        {
            comboBox1.Items.Add("Метод дихотомии");
            comboBox1.Items.Add("Метод золотого сечения");
            comboBox1.Items.Add("Метод ньютона");
            comboBox1.Items.Add("Сортировки");
            comboBox1.Items.Add("Вычисление определенного интегралла");
            comboBox1.Items.Add("Решение СЛАУ");
            comboBox1.Items.Add("Метод покоординатного спуска");
            comboBox1.Items.Add("Метод наименьших квадратов");

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.SelectedIndex = 0;
        }

        // ВЫПОЛНЕНИЯ ЗАДАНИЯ ПРИ ВЫБОРЕ
        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0: // Дихотомия
                    InitializeDichotomyControls();
                    break;
                case 1: // Золотое сечение
                    InitializeGoldenRatioControls();
                    break;
                case 2: // Метод ньютона
                    InitializeNewtonMethodControls();
                    break;
                case 3: // Cортировки
                    InitializeSortingControls();
                    break;
                case 4: // Интеграллы
                    InitializeDifiniteIntegralControls();
                    break;
                case 5: // СЛАУ
                    InitializeSLAU();
                    break;
                case 6: // Метод покоординатного спуска
                    InitializeCoordinateDescentControls();
                    break;
                case 7: // Метод Наименьших квадратов
                    InitializeLeastSquares();
                    break;
            }
        }

        // ПАНЕЛЬ 
        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void ResetAllPoints()
        {
            lastRoot = 0;
            lastMin = 0;
            lastMax = 0;
            lastZero = 0;
            newtonIterationPoints?.Clear();
        }

        //*******************************************************************************************| МЕТОД ДИХОТОМИИ |************************************************************************************//

        // ИНТЕРФЕЙС (МЕТОДА ДИХОТОМИИ)
        private void InitializeDichotomyControls()
        {
            // Очищаем панель
            panel1.Controls.Clear();

            // Создаем элементы 
            var labelFunction = new Label { Text = "Функция f(x):", Location = new Point(10, 10) };
            var textBoxFunction = new System.Windows.Forms.TextBox { Location = new Point(120, 10), Width = 120, Text = "x-1" };

            var labelA = new Label { Text = "Начало интервала a:", Location = new Point(10, 40) };
            var textBoxA = new System.Windows.Forms.TextBox { Location = new Point(120, 40), Width = 120, Text = "-5" };

            var labelB = new Label { Text = "Конец интервала b:", Location = new Point(10, 70) };
            var textBoxB = new System.Windows.Forms.TextBox { Location = new Point(120, 70), Width = 120, Text = "5" };

            var labelEpsilon = new Label { Text = "Точность:", Location = new Point(10, 100) };
            var textBoxEpsilon = new System.Windows.Forms.TextBox { Location = new Point(120, 100), Width = 120, Text = "5" };

            var calculateButton = new System.Windows.Forms.Button
            {
                Text = "Вычислить",
                Location = new Point(10, 130),
                BackColor = Color.LightBlue
            };

            var resultLabel = new Label
            {
                Text = "Результат:",
                Location = new Point(10, 170),
                AutoSize = true
            };

            var labelGraphic = new Label { Text = "График f(x)", Location = new Point(300, 10) };

            var graficPanel = new System.Windows.Forms.Panel { Location = new Point(300, 40), Size = new Size(400, 300), BorderStyle = BorderStyle.FixedSingle };

            // Добавляем обработчик события Paint
            graficPanel.Paint += (sender, e) =>
            {
                DrawGraph(e.Graphics, graficPanel.ClientRectangle);
            };

            // Кнопка для перерисовки графика
            var drawButton = new System.Windows.Forms.Button
            {
                Text = "Построить график",
                Location = new Point(120, 130),
                BackColor = Color.LightGreen,
                Width = 120
            };

            // Обработчик для кнопки построения графика
            drawButton.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(textBoxFunction.Text))
                {
                    currentFunction = textBoxFunction.Text;
                    graficPanel.Invalidate(); // Перерисовываем график
                }
            };

            calculateButton.Click += (s, e) => CalculateDichotomy(
                textBoxFunction.Text,
                textBoxA.Text,
                textBoxB.Text,
                textBoxEpsilon.Text,
                resultLabel);

            // После вычисления обновляем график
            if (!string.IsNullOrEmpty(textBoxFunction.Text))
            {
                currentFunction = textBoxFunction.Text;
                graficPanel.Invalidate();
            }

            // Добавляем элементы 
            panel1.Controls.AddRange(new Control[] { labelFunction, textBoxFunction, labelA, textBoxA, labelB, textBoxB, labelEpsilon, textBoxEpsilon, calculateButton, resultLabel, labelGraphic, graficPanel, drawButton });
        }

        // МЕТОД ДЛЯ ОТРИСОВКИ ГРАФИКА
        private void DrawGraph(Graphics g, Rectangle drawingArea)
        {
            if (string.IsNullOrEmpty(currentFunction))
                return;

            try
            {
                // Очищаем фон
                g.Clear(Color.White);

                // Если функция не задана, рисуем сообщение
                if (string.IsNullOrEmpty(currentFunction))
                {
                    g.DrawString("Введите функцию и нажмите 'Построить график'",
                        new Font("Arial", 10), Brushes.Gray, drawingArea.Width / 2 - 150, drawingArea.Height / 2 - 10);
                    return;
                }

                // лучшего качества
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Определяем область графика с отступами
                int padding = 30;
                Rectangle graphArea = new Rectangle(
                    drawingArea.Left + padding,
                    drawingArea.Top + padding,
                    drawingArea.Width - 2 * padding,
                    drawingArea.Height - 2 * padding
                );

                // Сетка
                DrawGrid(g, graphArea);

                // Оси координат
                DrawAxes(g, graphArea);

                // График функции
                DrawFunction(g, graphArea, currentFunction);

                // Оси
                DrawLabels(g, graphArea, drawingArea);
            }
            catch (Exception ex)
            {
                g.DrawString($"Ошибка построения графика: {ex.Message}",
                    new Font("Arial", 10), Brushes.Red, 10, 10);
            }
        }

        // ОТРИСОВКА СЕТКИ - исправленная версия
        private void DrawGrid(Graphics g, Rectangle graphArea)
        {
            Pen gridPen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dot };

            // Центр координат
            PointF center = new PointF(
                graphArea.Left + graphArea.Width / 2,
                graphArea.Top + graphArea.Height / 2
            );

            // Вертикальные линии сетки (не поверх осей)
            for (int i = -10; i <= 10; i++)
            {
                if (i == 0) continue; // Пропускаем ось Y

                float x = center.X + i * (graphArea.Width / 20f);
                g.DrawLine(gridPen, x, graphArea.Top, x, graphArea.Bottom);
            }

            // Горизонтальные линии сетки (не поверх осей)
            for (int i = -10; i <= 10; i++)
            {
                if (i == 0) continue; // Пропускаем ось X

                float y = center.Y - i * (graphArea.Height / 20f);
                g.DrawLine(gridPen, graphArea.Left, y, graphArea.Right, y);
            }
        }

        // ОТРИСОВКА ОСЕЙ КООРДИНАТ - исправленная версия
        private void DrawAxes(Graphics g, Rectangle graphArea)
        {
            Pen axisPen = new Pen(Color.Black, 2);

            // Центр координат
            PointF center = new PointF(
                graphArea.Left + graphArea.Width / 2,
                graphArea.Top + graphArea.Height / 2
            );

            // Ось X (горизонтальная)
            g.DrawLine(axisPen, graphArea.Left, center.Y, graphArea.Right, center.Y);

            // Стрелка оси X
            g.DrawLine(axisPen, graphArea.Right - 10, center.Y - 5, graphArea.Right, center.Y);
            g.DrawLine(axisPen, graphArea.Right - 10, center.Y + 5, graphArea.Right, center.Y);

            // Ось Y (вертикальная)
            g.DrawLine(axisPen, center.X, graphArea.Top, center.X, graphArea.Bottom);

            // Стрелка оси Y
            g.DrawLine(axisPen, center.X - 5, graphArea.Top + 10, center.X, graphArea.Top);
            g.DrawLine(axisPen, center.X + 5, graphArea.Top + 10, center.X, graphArea.Top);
        }

        // ОТРИСОВКА ГРАФИКА ФУНКЦИИ - с обработкой разрывов
        private void DrawFunction(Graphics g, Rectangle graphArea, string functionStr)
        {
            try
            {
                // Масштаб: определяем диапазон отображения
                float scaleX = graphArea.Width / 20f; // Отображаем от -10 до 10 по X
                float scaleY = graphArea.Height / 20f; // Отображаем от -10 до 10 по Y

                // Центр координат
                PointF center = new PointF(
                    graphArea.Left + graphArea.Width / 2,
                    graphArea.Top + graphArea.Height / 2
                );

                using (Pen graphPen = new Pen(Color.Blue, 2))
                {
                    List<PointF> segmentPoints = new List<PointF>();
                    bool lastPointValid = false;

                    // Увеличиваем количество точек для лучшего качества
                    int pointCount = graphArea.Width * 2;
                    double xStep = 20.0 / pointCount;

                    for (int i = 0; i <= pointCount; i++)
                    {
                        double worldX = -10 + i * xStep;

                        // Особая обработка для функций с разрывами (1/x)
                        if (functionStr.Contains("1/x") || functionStr.Contains("1/(x)"))
                        {
                            // Исключаем окрестность точки разрыва
                            if (Math.Abs(worldX) < 0.1) // Пропускаем x близкие к 0
                            {
                                lastPointValid = false;
                                continue;
                            }
                        }

                        try
                        {
                            double worldY = EvaluateMathExpression(functionStr, worldX);

                            // Ограничиваем слишком большие значения (для 1/x)
                            if (Math.Abs(worldY) > 100)
                            {
                                worldY = double.IsPositiveInfinity(worldY) ? 100 :
                                         double.IsNegativeInfinity(worldY) ? -100 :
                                         Math.Sign(worldY) * 100;
                            }

                            // Преобразуем мировые координаты в экранные
                            float screenX = center.X + (float)(worldX * scaleX);
                            float screenY = center.Y - (float)(worldY * scaleY);

                            // Проверяем, находится ли точка в видимой области
                            if (screenY >= graphArea.Top - 500 && screenY <= graphArea.Bottom + 500)
                            {
                                if (lastPointValid && segmentPoints.Count > 0)
                                {
                                    // Проверяем разрыв (резкий скачок)
                                    PointF lastPoint = segmentPoints.Last();
                                    float deltaY = Math.Abs(screenY - lastPoint.Y);

                                    if (deltaY < graphArea.Height * 0.8) // Разумный порог для разрыва
                                    {
                                        segmentPoints.Add(new PointF(screenX, screenY));
                                    }
                                    else
                                    {
                                        // Рисуем текущий сегмент и начинаем новый
                                        if (segmentPoints.Count >= 2)
                                        {
                                            g.DrawLines(graphPen, segmentPoints.ToArray());
                                        }
                                        segmentPoints.Clear();
                                        segmentPoints.Add(new PointF(screenX, screenY));
                                    }
                                }
                                else
                                {
                                    segmentPoints.Add(new PointF(screenX, screenY));
                                    lastPointValid = true;
                                }
                            }
                            else
                            {
                                lastPointValid = false;
                                // Рисуем накопленный сегмент
                                if (segmentPoints.Count >= 2)
                                {
                                    g.DrawLines(graphPen, segmentPoints.ToArray());
                                }
                                segmentPoints.Clear();
                            }
                        }
                        catch (DivideByZeroException)
                        {
                            // Разрыв функции - начинаем новый сегмент
                            lastPointValid = false;
                            if (segmentPoints.Count >= 2)
                            {
                                g.DrawLines(graphPen, segmentPoints.ToArray());
                            }
                            segmentPoints.Clear();
                        }
                        catch
                        {
                            lastPointValid = false;
                        }
                    }

                    // Рисуем последний сегмент
                    if (segmentPoints.Count >= 2)
                    {
                        g.DrawLines(graphPen, segmentPoints.ToArray());
                    }
                }

                // Отмечаем корень, если он найден
                if (lastRoot != 0)
                {
                    float rootX = center.X + (float)(lastRoot * scaleX);
                    float rootY = center.Y;

                    if (rootX >= graphArea.Left && rootX <= graphArea.Right)
                    {
                        // Рисуем точку корня
                        g.FillEllipse(Brushes.Red, rootX - 4, rootY - 4, 8, 8);
                        g.DrawEllipse(Pens.DarkRed, rootX - 4, rootY - 4, 8, 8);

                        // Подписываем корень
                        string rootText = $"x ≈ {lastRoot:F3}";
                        SizeF textSize = g.MeasureString(rootText, new Font("Arial", 9));
                        g.DrawString(rootText, new Font("Arial", 9), Brushes.Red,
                            rootX + 5, rootY - textSize.Height - 5);
                    }
                }
            }
            catch (Exception ex)
            {
                g.DrawString($"Ошибка построения: {ex.Message}",
                    new Font("Arial", 10), Brushes.Red, 10, 10);
            }
        }

        // ОТРИСОВКА ПОДПИСЕЙ ОСЕЙ И ДЕЛЕНИЙ - исправленная версия
        private void DrawLabels(Graphics g, Rectangle graphArea, Rectangle drawingArea)
        {
            Font labelFont = new Font("Arial", 9);
            Brush labelBrush = Brushes.Black;

            // Центр координат
            PointF center = new PointF(
                graphArea.Left + graphArea.Width / 2,
                graphArea.Top + graphArea.Height / 2
            );

            // Подпись оси X (рисуем ПЕРЕД делениями)
            g.DrawString("X", new Font("Arial", 10, FontStyle.Bold), Brushes.Black,
                graphArea.Right - 15, center.Y + 10);

            // Подпись оси Y (рисуем ПЕРЕД делениями)
            g.DrawString("Y", new Font("Arial", 10, FontStyle.Bold), Brushes.Black,
                center.X + 10, graphArea.Top);

            // Подписи делений на оси X (рисуем ПОСЛЕ осей)
            for (int i = -10; i <= 10; i += 2)
            {
                if (i == 0) continue;

                float x = center.X + i * (graphArea.Width / 20f);

                // Рисуем метку деления (короткая черточка)
                g.DrawLine(Pens.Black, x, center.Y - 3, x, center.Y + 3);

                // Подписываем значение
                string label = i.ToString();
                SizeF textSize = g.MeasureString(label, labelFont);
                g.DrawString(label, labelFont, labelBrush,
                    x - textSize.Width / 2, center.Y + 5);
            }

            // Подписи делений на оси Y (рисуем ПОСЛЕ осей)
            for (int i = -10; i <= 10; i += 2)
            {
                if (i == 0) continue;

                float y = center.Y - i * (graphArea.Height / 20f);

                // Рисуем метку деления
                g.DrawLine(Pens.Black, center.X - 3, y, center.X + 3, y);

                // Подписываем значение
                string label = i.ToString();
                SizeF textSize = g.MeasureString(label, labelFont);
                g.DrawString(label, labelFont, labelBrush,
                    center.X - textSize.Width - 5, y - textSize.Height / 2);
            }

            // Начало координат (0) - рисуем отдельно
            string zeroLabel = "0";
            SizeF zeroSize = g.MeasureString(zeroLabel, labelFont);
            g.DrawString(zeroLabel, labelFont, labelBrush,
                center.X + 3, center.Y + 3);

            // Подпись функции в углу
            if (!string.IsNullOrEmpty(currentFunction))
            {
                string functionLabel = $"f(x) = {currentFunction}";
                g.DrawString(functionLabel,
                    new Font("Arial", 10, FontStyle.Bold), Brushes.DarkBlue,
                    graphArea.Left, drawingArea.Top + 5);
            }
        }

        // РАСЧЕТЫ МЕТОДА ДИХОТОМИИ
        private void CalculateDichotomy(string functionStr, string aStr, string bStr, string epsilonStr, Label resultLabel)
        {
            ResetAllPoints(); 
            double accuracy = 1;            

            try
            {
                double a = double.Parse(aStr);
                double b = double.Parse(bStr);
                double epsilon = double.Parse(epsilonStr);

                if (epsilon < 1)
                {
                    resultLabel.Text = "Ошибка: Точность должна быть не меньше 1 знака после запятой!";
                    return;
                }

                accuracy = 1 / Math.Pow(10, epsilon);

                if (a >= b)
                {
                    resultLabel.Text = "Ошибка: 'a' должно быть меньше 'b'!";
                    return;
                }

                // f(a)*f(b) < 0
                double fa = EvaluateMathExpression(functionStr, a);
                double fb = EvaluateMathExpression(functionStr, b);

                if (fa * fb >= 0)
                {
                    resultLabel.Text = "На интервале нет 0 функции, либо корней больше 1!";
                    return;
                }

                // Реализация 
                int iterations = 0;
                double x0 = 0;

                while (Math.Abs(b - a) > accuracy)
                {
                    iterations++;
                    x0 = (a + b) / 2;
                    double fx0 = EvaluateMathExpression(functionStr, x0);

                    double currentFa = EvaluateMathExpression(functionStr, a);

                    if (currentFa * fx0 < 0)
                        b = x0;
                    else
                        a = x0;
                }

                // Сохраняем текущую функцию и найденный корень
                currentFunction = functionStr;
                lastRoot = x0;

                // Обновляем результат 
                resultLabel.Text = $"Результат: x = {x0:F3}\nИтераций: {iterations}";
            }
            catch (Exception ex)
            {
                resultLabel.Text = $"Ошибка: {ex.Message}";
            }
        }

        // ДИХОТОМИЯ РАСЧЕТ МАТЕМАТИЧЕСКИХ ВЫРАЖЕНИЙ
        private double EvaluateMathExpression(string expression, double x)
        {
            try
            {
                // Обработка деления на ноль для 1/x
                if ((expression.Contains("1/x") || expression.Contains("1/(x)")) && Math.Abs(x) < 1e-10)
                {
                    throw new DivideByZeroException("Деление на ноль");
                }

                var parser = new MathParser();
                parser.LocalVariables["x"] = x;
                parser.LocalVariables["pi"] = Math.PI;
                parser.LocalVariables["e"] = Math.E;

                expression = PreprocessExpression(expression);
                return parser.Parse(expression);
            }
            catch (DivideByZeroException)
            {
                throw; // Пробрасываем специальное исключение для разрывов
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Ошибка вычисления '{expression}': {ex.Message}");
            }
        }

        // ОБРАБОТКА ВЫРАЖЕНИЙ
        private string PreprocessExpression(string expression)
        {
            expression = expression.Replace(" ", "");

            // Формат Mathos
            expression = expression.ToLower();

            // замены
            var replacements = new Dictionary<string, string>
            {
                { "^", "^" },
                { "sqrt", "sqrt" },
                { "ln", "ln" },
                { "log", "log10" },
                { "abs", "abs" },
                { "exp", "exp" },
                { "sin", "sin" },
                { "cos", "cos" },
                { "tan", "tan" },
                { "tg", "tan" },
                { "ctg", "cot" },
                { "arcsin", "asin" },
                { "arccos", "acos" },
                { "arctan", "atan" },
                { "arctg", "atan" },
                { "log2", "log2" },
                { "pi", "pi" },
                { "e", "e" },
            };


            // Проходим по всем заменам
            foreach (var replacement in replacements)
            {
                expression = expression.Replace(replacement.Key, replacement.Value);
            }

            // Mathos (2x = 2*x)
            expression = InsertMultiplicationSigns(expression);

            return expression;
        }

        // ДИХОТОМИЯ УДАЛЕНИЕ ПРОБЕЛОВ
        private string InsertMultiplicationSigns(string expression)
        {
            var result = new StringBuilder();
            for (int i = 0; i < expression.Length; i++)
            {
                result.Append(expression[i]);

                if (i < expression.Length - 1)
                {
                    char current = expression[i];
                    char next = expression[i + 1];

                    if ((char.IsDigit(current) || current == 'x' || current == ')') &&
                        (next == 'x' || next == '(' || char.IsLetter(next)))
                    {
                        result.Append('*');
                    }
                    else if (current == 'x' && (next == '(' || char.IsLetter(next)))
                    {
                        result.Append('*');
                    }
                    else if (char.IsDigit(current) && char.IsDigit(next))
                    {
                        // Оставляем как есть для десятичных чисел
                        if (current != '.' && next != '.')
                        {
                            // Это не десятичное число, но возможно, это отдельные числа
                            // В этом случае добавляем "*"
                            result.Append('*');
                        }
                    }
                }
            }

            return result.ToString();
        }

        //***************************************************************************************| МЕТОД ЗОЛОТОГО СЕЧЕНИЯ |***********************************************************************************//

        // ИНТЕРФЕЙС ЗОЛОТОГО СЕЧЕНИЯ
        private void InitializeGoldenRatioControls()
        {
            panel1.Controls.Clear();

            // Элементы управления
            var labelFunction = new Label { Text = "Функция f(x):", Location = new Point(10, 10) };
            var textBoxFunction = new System.Windows.Forms.TextBox { Location = new Point(120, 10), Width = 120, Text = "x^2-2" };

            var labelA = new Label { Text = "Начало интервала a:", Location = new Point(10, 40) };
            var textBoxA = new System.Windows.Forms.TextBox { Location = new Point(120, 40), Width = 120, Text = "-5" };

            var labelB = new Label { Text = "Конец интервала b:", Location = new Point(10, 70) };
            var textBoxB = new System.Windows.Forms.TextBox { Location = new Point(120, 70), Width = 120, Text = "5" };

            var labelEpsilon = new Label { Text = "Точность ε:", Location = new Point(10, 100) };
            var textBoxEpsilon = new System.Windows.Forms.TextBox { Location = new Point(120, 100), Width = 120, Text = "1" };

            // Кнопки для разных операций
            var btnFindMin = new System.Windows.Forms.Button
            {
                Text = "Найти минимум",
                Location = new Point(10, 130),
                BackColor = Color.LightBlue,
                Width = 120
            };

            var btnFindMax = new System.Windows.Forms.Button
            {
                Text = "Найти максимум",
                Location = new Point(10, 160),
                BackColor = Color.LightCoral,
                Width = 120
            };

            // Поле для результатов
            var resultLabel = new Label
            {
                Text = "Результат:",
                Location = new Point(10, 220),
                AutoSize = true,
                Height = 60,
                Width = 250
            };

            // Панель для графика
            var labelGraphic = new Label { Text = "График f(x)", Location = new Point(300, 10) };

            graficPanel = new System.Windows.Forms.Panel
            {
                Location = new Point(300, 40),
                Size = new Size(400, 300),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Обработчик отрисовки графика
            graficPanel.Paint += (sender, e) =>
            {
                DrawGraphForGoldenRatio(e.Graphics, graficPanel.ClientRectangle);
            };

            // Кнопка для построения графика
            var drawButton = new System.Windows.Forms.Button
            {
                Text = "Построить график",
                Location = new Point(150, 160),
                BackColor = Color.LightYellow,
                Width = 120
            };
            
            // Найти все
            var btnFindAll = new System.Windows.Forms.Button
            {
                Text = "Найти все точки",
                Location = new Point(150, 130),
                BackColor = Color.Purple,
                ForeColor = Color.White,
                Width = 120
            };

            btnFindAll.Click += (s, e) => CalculateAllPoints(
                textBoxFunction.Text,
                textBoxA.Text,
                textBoxB.Text,
                textBoxEpsilon.Text,
                resultLabel);

            // Обработчики событий
            drawButton.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(textBoxFunction.Text))
                {
                    currentFunction = textBoxFunction.Text;
                    graficPanel.Invalidate();
                }
            };

            btnFindMin.Click += (s, e) => FindMinimum(
                textBoxFunction.Text,
                textBoxA.Text,
                textBoxB.Text,
                textBoxEpsilon.Text,
                resultLabel);

            btnFindMax.Click += (s, e) => FindMaximum(
                textBoxFunction.Text,
                textBoxA.Text,
                textBoxB.Text,
                textBoxEpsilon.Text,
                resultLabel);

            // Добавление элементов на панель
            panel1.Controls.AddRange(new Control[]
            {
        labelFunction, textBoxFunction,
        labelA, textBoxA,
        labelB, textBoxB,
        labelEpsilon, textBoxEpsilon,
        btnFindMin, btnFindMax,
        resultLabel, labelGraphic, graficPanel, drawButton, btnFindAll
            });
        }

        // Метод золотого сечения для поиска минимума
        private void FindMinimum(string functionStr, string aStr, string bStr, string epsilonStr, Label resultLabel)
        {
            double accuracy = 1;
            try
            {
                double a = double.Parse(aStr);
                double b = double.Parse(bStr);
                double epsilon = double.Parse(epsilonStr);

                if (epsilon < 1)
                {
                    resultLabel.Text = "Ошибка: Точность должна быть не меньше 1 знака после запятой!";
                    return;
                }
                
                accuracy = 1 / Math.Pow(10, epsilon);

                if (a >= b)
                {
                    resultLabel.Text = "Ошибка: 'a' должно быть меньше 'b'!";
                    return;
                }

                // Золотое сечение
                double phi = (1 + Math.Sqrt(5)) / 2;
                double resPhi = 2 - phi;

                double x1 = a + resPhi * (b - a);
                double x2 = b - resPhi * (b - a);

                double f1 = EvaluateMathExpression(functionStr, x1);
                double f2 = EvaluateMathExpression(functionStr, x2);

                int iterations = 0;
                int maxIterations = 1000;

                while (Math.Abs(b - a) > accuracy && iterations < maxIterations)
                {
                    iterations++;

                    if (f1 < f2)
                    {
                        b = x2;
                        x2 = x1;
                        f2 = f1;
                        x1 = a + resPhi * (b - a);
                        f1 = EvaluateMathExpression(functionStr, x1);
                    }
                    else
                    {
                        a = x1;
                        x1 = x2;
                        f1 = f2;
                        x2 = b - resPhi * (b - a);
                        f2 = EvaluateMathExpression(functionStr, x2);
                    }
                }

                double xMin = (a + b) / 2;
                double fMin = EvaluateMathExpression(functionStr, xMin);

                lastMin = xMin;
                currentFunction = functionStr;
                graficPanel.Invalidate();

                resultLabel.Text = $"МИНИМУМ найден:\n" +
                                  $"x = {xMin:F6}\n" +
                                  $"f(x) = {fMin:F6}\n" +
                                  $"Итераций: {iterations}";
            }
            catch (Exception ex)
            {
                resultLabel.Text = $"Ошибка: {ex.Message}";
            }
        }

        // Метод золотого сечения для поиска максимума
        private void FindMaximum(string functionStr, string aStr, string bStr, string epsilonStr, Label resultLabel)
        {
            double accuracy = 1;
            try
            {
                double a = double.Parse(aStr);
                double b = double.Parse(bStr);
                double epsilon = double.Parse(epsilonStr);

                if (epsilon < 1)
                {
                    resultLabel.Text = "Ошибка: Точность должна быть не меньше 1 знака после запятой!";
                    return;
                }

                accuracy = 1 / Math.Pow(10, epsilon);

                if (a >= b)
                {
                    resultLabel.Text = "Ошибка: 'a' должно быть меньше 'b'!";
                    return;
                }

                // Для поиска максимума ищем минимум от -f(x)
                string negativeFunction = "-(" + functionStr + ")";

                double phi = (1 + Math.Sqrt(5)) / 2;
                double resPhi = 2 - phi;

                double x1 = a + resPhi * (b - a);
                double x2 = b - resPhi * (b - a);

                double f1 = EvaluateMathExpression(negativeFunction, x1);
                double f2 = EvaluateMathExpression(negativeFunction, x2);

                int iterations = 0;
                int maxIterations = 1000;

                while (Math.Abs(b - a) > accuracy && iterations < maxIterations)
                {
                    iterations++;

                    if (f1 < f2)
                    {
                        b = x2;
                        x2 = x1;
                        f2 = f1;
                        x1 = a + resPhi * (b - a);
                        f1 = EvaluateMathExpression(negativeFunction, x1);
                    }
                    else
                    {
                        a = x1;
                        x1 = x2;
                        f1 = f2;
                        x2 = b - resPhi * (b - a);
                        f2 = EvaluateMathExpression(negativeFunction, x2);
                    }
                }

                double xMax = (a + b) / 2;
                double fMax = EvaluateMathExpression(functionStr, xMax);

                lastMax = xMax;
                currentFunction = functionStr;
                graficPanel.Invalidate();

                resultLabel.Text = $"МАКСИМУМ найден:\n" +
                                  $"x = {xMax:F6}\n" +
                                  $"f(x) = {fMax:F6}\n" +
                                  $"Итераций: {iterations}";
            }
            catch (Exception ex)
            {
                resultLabel.Text = $"Ошибка: {ex.Message}";
            }
        }

        // Отрисовка графика
        private void DrawGraphForGoldenRatio(Graphics g, Rectangle drawingArea)
        {
            if (string.IsNullOrEmpty(currentFunction))
                return;

            try
            {
                // Очищаем фон
                g.Clear(Color.White);

                // Качество
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Область графика с отступами
                int padding = 40;
                Rectangle graphArea = new Rectangle(
                    drawingArea.Left + padding,
                    drawingArea.Top + padding,
                    drawingArea.Width - 2 * padding,
                    drawingArea.Height - 2 * padding
                );

                // Рисуем сетку
                DrawGrid(g, graphArea);

                // Рисуем оси
                DrawAxes(g, graphArea);

                // Рисуем график функции
                DrawFunctionForGoldenRatio(g, graphArea, currentFunction);

                // Рисуем подписи
                DrawLabelsForGoldenRatio(g, graphArea, drawingArea);
            }
            catch (Exception ex)
            {
                g.DrawString($"Ошибка построения графика: {ex.Message}",
                    new Font("Arial", 10), Brushes.Red, 10, 10);
            }
        }
        private void DrawFunctionForGoldenRatio(Graphics g, Rectangle graphArea, string functionStr)
        {
            try
            {
                // Масштаб
                float scaleX = graphArea.Width / 20f; // -10 до 10
                float scaleY = graphArea.Height / 20f; // -10 до 10

                // Центр координат
                PointF center = new PointF(
                    graphArea.Left + graphArea.Width / 2,
                    graphArea.Top + graphArea.Height / 2
                );

                using (Pen graphPen = new Pen(Color.Blue, 2))
                {
                    List<PointF> points = new List<PointF>();
                    PointF? lastPoint = null;

                    // Генерируем точки графика
                    for (int i = 0; i <= graphArea.Width; i++)
                    {
                        double worldX = -10 + (20.0 * i / graphArea.Width);

                        try
                        {
                            double worldY = EvaluateMathExpression(functionStr, worldX);

                            float screenX = center.X + (float)(worldX * scaleX);
                            float screenY = center.Y - (float)(worldY * scaleY);

                            PointF currentPoint = new PointF(screenX, screenY);

                            if (lastPoint.HasValue)
                            {
                                // Рисуем линию от предыдущей точки к текущей
                                g.DrawLine(graphPen, lastPoint.Value, currentPoint);
                            }

                            lastPoint = currentPoint;
                        }
                        catch
                        {
                            lastPoint = null;
                        }
                    }

                    // Отмечаем особые точки
                    DrawSpecialPoints(g, graphArea, center, scaleX, scaleY);
                }
            }
            catch (Exception ex)
            {
                // Игнорируем ошибки отрисовки
            }
        }
        private void DrawSpecialPoints(Graphics g, Rectangle graphArea, PointF center, float scaleX, float scaleY)
        {
            // Отмечаем минимум
            if (Math.Abs(lastMin) > 0.001)
            {
                try
                {
                    float x = center.X + (float)(lastMin * scaleX);
                    float y = center.Y - (float)(EvaluateMathExpression(currentFunction, lastMin) * scaleY);

                    if (x >= graphArea.Left && x <= graphArea.Right && y >= graphArea.Top && y <= graphArea.Bottom)
                    {
                        g.FillEllipse(Brushes.Green, x - 5, y - 5, 10, 10);
                        g.DrawEllipse(Pens.DarkGreen, x - 5, y - 5, 10, 10);

                        string label = $"min({lastMin:F2})";
                        SizeF textSize = g.MeasureString(label, new Font("Arial", 8));
                        g.DrawString(label, new Font("Arial", 8), Brushes.DarkGreen,
                            x + 6, y - textSize.Height / 2);
                    }
                }
                catch { }
            }

            // Отмечаем максимум
            if (Math.Abs(lastMax) > 0.001)
            {
                try
                {
                    float x = center.X + (float)(lastMax * scaleX);
                    float y = center.Y - (float)(EvaluateMathExpression(currentFunction, lastMax) * scaleY);

                    if (x >= graphArea.Left && x <= graphArea.Right && y >= graphArea.Top && y <= graphArea.Bottom)
                    {
                        g.FillEllipse(Brushes.Red, x - 5, y - 5, 10, 10);
                        g.DrawEllipse(Pens.DarkRed, x - 5, y - 5, 10, 10);

                        string label = $"max({lastMax:F2})";
                        SizeF textSize = g.MeasureString(label, new Font("Arial", 8));
                        g.DrawString(label, new Font("Arial", 8), Brushes.DarkRed,
                            x + 6, y - textSize.Height / 2);
                    }
                }
                catch { }
            }

            // Отмечаем ноль функции
            if (Math.Abs(lastZero) > 0.001)
            {
                try
                {
                    float x = center.X + (float)(lastZero * scaleX);
                    float y = center.Y; // y = 0

                    if (x >= graphArea.Left && x <= graphArea.Right)
                    {
                        g.FillEllipse(Brushes.Orange, x - 5, y - 5, 10, 10);
                        g.DrawEllipse(Pens.DarkOrange, x - 5, y - 5, 10, 10);

                        string label = $"zero({lastZero:F2})";
                        SizeF textSize = g.MeasureString(label, new Font("Arial", 8));
                        g.DrawString(label, new Font("Arial", 8), Brushes.DarkOrange,
                            x + 6, y - textSize.Height / 2);
                    }
                }
                catch { }
            }
        }
        private void DrawLabelsForGoldenRatio(Graphics g, Rectangle graphArea, Rectangle drawingArea)
        {
            Font labelFont = new Font("Arial", 9);
            Font titleFont = new Font("Arial", 10, FontStyle.Bold);

            PointF center = new PointF(
                graphArea.Left + graphArea.Width / 2,
                graphArea.Top + graphArea.Height / 2
            );

            // Подписи осей
            g.DrawString("X", titleFont, Brushes.Black, graphArea.Right - 15, center.Y - 20);
            g.DrawString("Y", titleFont, Brushes.Black, center.X + 10, graphArea.Top);

            // Подпись функции
            if (!string.IsNullOrEmpty(currentFunction))
            {
                string title = $"f(x) = {currentFunction}";
                SizeF titleSize = g.MeasureString(title, titleFont);
                g.DrawString(title, titleFont, Brushes.DarkBlue,
                    drawingArea.Left + 10, drawingArea.Top + 10);
            }

            // Легенда
            if (Math.Abs(lastMin) > 0.001 || Math.Abs(lastMax) > 0.001 || Math.Abs(lastZero) > 0.001)
            {
                string legend = "Обозначения:";
                if (Math.Abs(lastMin) > 0.001) legend += " ● min - минимум";
                if (Math.Abs(lastMax) > 0.001) legend += " ● max - максимум";
                if (Math.Abs(lastZero) > 0.001) legend += " ● zero - ноль функции";

                g.DrawString(legend, new Font("Arial", 8), Brushes.DarkGray,
                    graphArea.Left, graphArea.Bottom + 5);
            }
        }
        
        // Все сразу
        private void CalculateAllPoints(string functionStr, string aStr, string bStr, string epsilonStr, Label resultLabel)
        {
            try
            {
                double a = double.Parse(aStr);
                double b = double.Parse(bStr);
                double epsilon = double.Parse(epsilonStr);

                // Находим минимум
                FindMinimum(functionStr, aStr, bStr, epsilonStr, resultLabel);
                double minX = lastMin;
                double minY = EvaluateMathExpression(functionStr, minX);

                // Находим максимум
                FindMaximum(functionStr, aStr, bStr, epsilonStr, resultLabel);
                double maxX = lastMax;
                double maxY = EvaluateMathExpression(functionStr, maxX);

                // Находим ноль
                FindZero(functionStr, aStr, bStr, epsilonStr, resultLabel);
                double zeroX = lastZero;

                // Обновляем график
                currentFunction = functionStr;
                graficPanel.Invalidate();

                resultLabel.Text = $"Все точки найдены:\n" +
                                  $"Минимум: x={minX:F4}, f(x)={minY:F4}\n" +
                                  $"Максимум: x={maxX:F4}, f(x)={maxY:F4}\n" +
                                  $"Ноль: x={zeroX:F4}";
            }
            catch (Exception ex)
            {
                resultLabel.Text = $"Ошибка: {ex.Message}";
            }
        }
        // Поиск нуля функции (f(x) = 0)
        private void FindZero(string functionStr, string aStr, string bStr, string epsilonStr, Label resultLabel)
        {
            double accuracy = 1;
            try
            {
                double a = double.Parse(aStr);
                double b = double.Parse(bStr);
                double epsilon = double.Parse(epsilonStr);

                if (epsilon < 1)
                {
                    resultLabel.Text = "Ошибка: Точность должна быть не меньше 1 знака после запятой!";
                    return;
                }

                accuracy = 1 / Math.Pow(10, epsilon);

                if (a >= b)
                {
                    resultLabel.Text = "Ошибка: 'a' должно быть меньше 'b'!";
                    return;
                }

                // Проверяем, что на концах интервала функция имеет разные знаки
                double fa = EvaluateMathExpression(functionStr, a);
                double fb = EvaluateMathExpression(functionStr, b);

                if (Math.Abs(fa) < accuracy)
                {
                    lastZero = a;
                    currentFunction = functionStr;
                    graficPanel.Invalidate();
                    resultLabel.Text = $"НУЛЬ найден на левой границе:\nx = {a:F6}\nf(x) = 0";
                    return;
                }

                if (Math.Abs(fb) < accuracy)
                {
                    lastZero = b;
                    currentFunction = functionStr;
                    graficPanel.Invalidate();
                    resultLabel.Text = $"НУЛЬ найден на правой границе:\nx = {b:F6}\nf(x) = 0";
                    return;
                }

                if (fa * fb > 0)
                {
                    resultLabel.Text = "На интервале нет корня (f(a) и f(b) одного знака)!\n" +
                                      $"f({a}) = {fa:F6}, f({b}) = {fb:F6}";
                    return;
                }

                // Метод дихотомии для поиска нуля
                double left = a;
                double right = b;
                int iterations = 0;
                int maxIterations = 1000;

                while (Math.Abs(right - left) > accuracy && iterations < maxIterations)
                {
                    iterations++;
                    double mid = (left + right) / 2;
                    double fmid = EvaluateMathExpression(functionStr, mid);

                    if (Math.Abs(fmid) < accuracy)
                    {
                        lastZero = mid;
                        currentFunction = functionStr;
                        graficPanel.Invalidate();
                        resultLabel.Text = $"НУЛЬ найден:\nx = {mid:F6}\nf(x) = {fmid:E}\nИтераций: {iterations}";
                        return;
                    }

                    if (fa * fmid < 0)
                        right = mid;
                    else
                    {
                        left = mid;
                        fa = fmid;
                    }
                }

                double xZero = (left + right) / 2;
                double fZero = EvaluateMathExpression(functionStr, xZero);

                lastZero = xZero;
                currentFunction = functionStr;
                graficPanel.Invalidate();

                resultLabel.Text = $"НУЛЬ найден:\n" +
                                  $"x ≈ {xZero:F6}\n" +
                                  $"f(x) ≈ {fZero:E}\n" +
                                  $"Точность: {epsilon}\n" +
                                  $"Итераций: {iterations}";
            }
            catch (Exception ex)
            {
                resultLabel.Text = $"Ошибка: {ex.Message}";
            }
        }

        //********************************************************************************************| МЕТОД НЬЮТОНА |****************************************************************************************//

        // ИНТЕРФЕЙС МЕТОДА НЬЮТОНА
        private void InitializeNewtonMethodControls()
        {
            // Очищаем панель
            panel1.Controls.Clear();

            // Создаем элементы управления
            var labelFunction = new Label { Text = "Функция f(x):", Location = new Point(10, 10) };
            var textBoxFunction = new System.Windows.Forms.TextBox { Location = new Point(120, 10), Width = 180, Text = "x^3 - 2*x - 5" };

            var labelDerivative = new Label { Text = "Производная f'(x):", Location = new Point(10, 40) };
            var textBoxDerivative = new System.Windows.Forms.TextBox { Location = new Point(120, 40), Width = 120, Text = "3*x^2 - 2" };

            // Кнопка для автоматического вычисления производной
            var btnAutoDerivative = new System.Windows.Forms.Button
            {
                Text = "Авто",
                Location = new Point(249, 39),
                Width = 50,
                BackColor = Color.LightGray
            };

            var labelX0 = new Label { Text = "Начальное x₀:", Location = new Point(10, 70) };
            var textBoxX0 = new System.Windows.Forms.TextBox { Location = new Point(120, 70), Width = 180, Text = "2" };

            var labelEpsilon = new Label { Text = "Точность", Location = new Point(10, 100) };
            var textBoxEpsilon = new System.Windows.Forms.TextBox { Location = new Point(120, 100), Width = 180, Text = "1" };

            var labelMaxIterations = new Label { Text = "Макс. итераций:", Location = new Point(10, 130) };
            var textBoxMaxIterations = new System.Windows.Forms.TextBox { Location = new Point(120, 130), Width = 180, Text = "10" };

            var calculateButton = new System.Windows.Forms.Button
            {
                Text = "Вычислить Минимум",
                Location = new Point(10, 160),
                BackColor = Color.LightBlue,
                Width = 140
            };

            var resultLabel = new Label
            {
                Text = "Результат:",
                Location = new Point(10, 190),
                AutoSize = true,
                Height = 120,
                Width = 280
            };

            var labelGraphic = new Label { Text = "График f(x) и касательные", Location = new Point(320, 10), AutoSize = true };

            var graficPanel = new Panel
            {
                Location = new Point(320, 40),
                Size = new Size(450, 380),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Таблица итераций
            var dataGridView = new DataGridView
            {
                Location = new Point(10, 270),
                Size = new Size(290, 150),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                RowHeadersVisible = false,
                ScrollBars = ScrollBars.Vertical
            };

            // Настраиваем столбцы таблицы
            dataGridView.Columns.Add("Iteration", "Итер.");
            dataGridView.Columns.Add("x", "xₙ");
            dataGridView.Columns.Add("f(x)", "f(xₙ)");
            dataGridView.Columns.Add("f'(x)", "f'(xₙ)");
            dataGridView.Columns.Add("Delta", "Δx");

            // Устанавливаем формат отображения чисел
            dataGridView.Columns["f(x)"].DefaultCellStyle.Format = "E4";
            dataGridView.Columns["f'(x)"].DefaultCellStyle.Format = "E4";
            dataGridView.Columns["Delta"].DefaultCellStyle.Format = "E4";

            // Обработчик для автоматического вычисления производной
            btnAutoDerivative.Click += (s, e) =>
            {
                try
                {
                    string function = textBoxFunction.Text.Trim();
                    if (!string.IsNullOrEmpty(function))
                    {
                        string derivative = CalculateDerivative(function);
                        textBoxDerivative.Text = derivative;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка вычисления производной: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Обработчик отрисовки графика
            graficPanel.Paint += (sender, e) =>
            {
                DrawNewtonGraph(e.Graphics, graficPanel.ClientRectangle,
                    textBoxFunction.Text, textBoxDerivative.Text);
            };

            // Кнопка для построения графика
            var drawButton = new System.Windows.Forms.Button
            {
                Text = "Построить график",
                Location = new Point(160, 160),
                BackColor = Color.LightGreen,
                Width = 140
            };

            drawButton.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(textBoxFunction.Text))
                {
                    currentFunction = textBoxFunction.Text;
                    graficPanel.Invalidate();
                }
            };

            // Обработчик вычисления
            calculateButton.Click += (s, e) =>
            {
                dataGridView.Rows.Clear();
                CalculateNewtonMethod(
                    textBoxFunction.Text,
                    textBoxDerivative.Text,
                    textBoxX0.Text,
                    textBoxEpsilon.Text,
                    textBoxMaxIterations.Text,
                    resultLabel,
                    dataGridView
                );
                graficPanel.Invalidate();
            };

            // Добавляем элементы на панель
            panel1.Controls.AddRange(new Control[]
            {
        labelFunction, textBoxFunction,
        labelDerivative, textBoxDerivative, btnAutoDerivative,
        labelX0, textBoxX0,
        labelEpsilon, textBoxEpsilon,
        labelMaxIterations, textBoxMaxIterations,
        calculateButton, drawButton,
        resultLabel, labelGraphic, graficPanel,
        dataGridView
            });
        }

        // производная
        private string CalculateDerivative(string function)
        {
            // Упрощенный метод вычисления производной (для основных функций)
            function = function.ToLower().Replace(" ", "");

            // Простейшие правила дифференцирования
            if (function.Contains("x^"))
            {
                // Попробуем извлечь степень
                int index = function.IndexOf("x^");
                if (index >= 0)
                {
                    string rest = function.Substring(index + 2);
                    if (double.TryParse(rest, out double power))
                    {
                        if (power == 1)
                            return "1";
                        else if (power == 2)
                            return "2*x";
                        else if (power == 3)
                            return "3*x^2";
                        else
                            return $"{power}*x^{power - 1}";
                    }
                }
            }
            else if (function == "x")
            {
                return "1";
            }
            else if (function == "sin(x)")
            {
                return "cos(x)";
            }
            else if (function == "cos(x)")
            {
                return "-sin(x)";
            }
            else if (function == "exp(x)" || function == "e^x")
            {
                return function; // Производная e^x равна e^x
            }
            else if (function == "ln(x)")
            {
                return "1/x";
            }

            // Если не удалось определить производную, возвращаем приближенную формулу
            return $"(f(x+0.001)-f(x))/0.001";
        }

        // Метод для вычисления второй производной (нужен для определения типа точки)
        private string CalculateSecondDerivative(string function)
        {
            // Сначала получаем первую производную
            string firstDeriv = CalculateDerivative(function);

            // Потом дифференцируем ее еще раз
            return CalculateDerivative(firstDeriv);
        }

        // МЕТОД НЬЮТОНА ДЛЯ НАХОЖДЕНИЯ МИНИМУМА (не корня!)
        private void CalculateNewtonMethod(string functionStr, string derivativeStr, string x0Str,
            string epsilonStr, string maxIterStr, Label resultLabel, DataGridView dataGridView)
        {
            double accuracy = 1;

            try
            {
                double x0 = double.Parse(x0Str);
                double epsilon = double.Parse(epsilonStr);
                int maxIterations = int.Parse(maxIterStr);

                if (epsilon < 1)
                {
                    resultLabel.Text = "Ошибка: Точность должна быть не меньше 1 знака после запятой!";
                    return;
                }

                accuracy = 1 / Math.Pow(10, epsilon);

                if (maxIterations <= 0)
                {
                    resultLabel.Text = "Ошибка: Максимальное число итераций должно быть больше 0!";
                    return;
                }

                double xn = x0;
                double fxn = EvaluateMathExpression(functionStr, xn);
                double fpxn = 0; // первая производная
                double fppxn = 0; // вторая производная (новая переменная)
                double delta = 0;
                int iteration = 0;
                bool converged = false;

                List<double> iterationPoints = new List<double>();
                iterationPoints.Add(xn);

                // Сохраняем для отрисовки
                currentFunction = functionStr;
                lastRoot = 0; // будем использовать как найденный минимум
                lastMin = 0; // тоже будем использовать

                // Основной цикл метода Ньютона для минимума
                while (iteration < maxIterations)
                {
                    // Вычисляем значение функции
                    fxn = EvaluateMathExpression(functionStr, xn);

                    // Вычисляем первую производную
                    if (derivativeStr.Contains("f(x+") && derivativeStr.Contains("f(x)"))
                    {
                        // Используем приближенную производную
                        double h = 0.0001;
                        double fxh = EvaluateMathExpression(functionStr, xn + h);
                        fpxn = (fxh - fxn) / h;
                    }
                    else
                    {
                        // Используем аналитическую производную
                        fpxn = EvaluateMathExpression(derivativeStr, xn);
                    }

                    // Вычисляем вторую производную (нужна для минимума)
                    // Используем численное дифференцирование первой производной
                    double h2 = 0.0001;
                    double fpxh = 0;

                    if (derivativeStr.Contains("f(x+") && derivativeStr.Contains("f(x)"))
                    {
                        // Если производная задана численно
                        double fxh1 = EvaluateMathExpression(functionStr, xn + h2);
                        double fxh2 = EvaluateMathExpression(functionStr, xn - h2);
                        fpxh = (fxh1 - fxh2) / (2 * h2);
                    }
                    else
                    {
                        // Вычисляем производную в соседних точках
                        double fpx_plus = EvaluateMathExpression(derivativeStr, xn + h2);
                        double fpx_minus = EvaluateMathExpression(derivativeStr, xn - h2);
                        fpxh = (fpx_plus - fpx_minus) / (2 * h2);
                    }

                    fppxn = fpxh; // это вторая производная

                    // Проверка на нулевую вторую производную
                    if (Math.Abs(fppxn) < accuracy)
                    {
                        resultLabel.Text = $"Ошибка: Вторая производная близка к нулю!\n" +
                                          $"f''({xn:F6}) = {fppxn:E}\n" +
                                          $"Итерация: {iteration}";
                        return;
                    }

                    // ФОРМУЛА МЕТОДА НЬЮТОНА ДЛЯ МИНИМУМА: x_{n+1} = x_n - f'(x_n)/f''(x_n)
                    // (а не x_{n+1} = x_n - f(x_n)/f'(x_n) как для корня)
                    double xn1 = xn - fpxn / fppxn;
                    delta = Math.Abs(xn1 - xn);

                    // Добавляем строку в таблицу
                    dataGridView.Rows.Add(iteration + 1,
                                         xn.ToString("F6"),
                                         fxn.ToString("E4"),
                                         fpxn.ToString("E4"),
                                         delta.ToString("E4"));

                    // Сохраняем точку для отрисовки
                    iterationPoints.Add(xn1);

                    // Проверка условия остановки для минимума: f'(x) близко к 0
                    if (Math.Abs(fpxn) < accuracy || delta < accuracy)
                    {
                        converged = true;
                        xn = xn1;
                        lastRoot = xn; // сохраняем найденную точку
                        lastMin = xn;  // тоже сохраняем как минимум
                        break;
                    }

                    xn = xn1;
                    iteration++;
                }

                // Пересчитываем значения в найденной точке
                fxn = EvaluateMathExpression(functionStr, xn);
                fpxn = EvaluateMathExpression(derivativeStr, xn);

                // Вычисляем вторую производную для проверки типа точки
                double finalFppxn = 0;
                try
                {
                    // Пытаемся вычислить аналитически
                    string secondDerivative = CalculateSecondDerivative(functionStr);
                    finalFppxn = EvaluateMathExpression(secondDerivative, xn);
                }
                catch
                {
                    // Используем численное дифференцирование
                    double h = 0.0001;
                    double fpx_plus = EvaluateMathExpression(derivativeStr, xn + h);
                    double fpx_minus = EvaluateMathExpression(derivativeStr, xn - h);
                    finalFppxn = (fpx_plus - fpx_minus) / (2 * h);
                }

                // Формируем результат
                if (converged)
                {
                    // Проверяем тип точки
                    if (finalFppxn > 0)
                    {
                        resultLabel.Text = $"МИНИМУМ найден:\n" +
                                          $"x = {xn:F8}\n" +
                                          $"f(x) = {fxn:F8}\n" +
                                          $"f'(x) = {fpxn:E} (≈0)\n" +
                                          $"f''(x) = {finalFppxn:F6} (>0 - минимум)\n" +
                                          $"Итераций: {iteration + 1}\n" +
                                          $"Точность: {epsilon}";
                    }
                    else if (finalFppxn < 0)
                    {
                        resultLabel.Text = $"МАКСИМУМ найден:\n" +
                                          $"x = {xn:F8}\n" +
                                          $"f(x) = {fxn:F8}\n" +
                                          $"f'(x) = {fpxn:E} (≈0)\n" +
                                          $"f''(x) = {finalFppxn:F6} (<0 - максимум)\n" +
                                          $"Итераций: {iteration + 1}\n" +
                                          $"Точность: {epsilon}";
                    }
                    else
                    {
                        resultLabel.Text = $"СТАЦИОНАРНАЯ ТОЧКА:\n" +
                                          $"x = {xn:F8}\n" +
                                          $"f(x) = {fxn:F8}\n" +
                                          $"f'(x) = {fpxn:E} (≈0)\n" +
                                          $"f''(x) = {finalFppxn:F6} (неопределено)\n" +
                                          $"Итераций: {iteration + 1}\n" +
                                          $"Точность: {epsilon}";
                    }

                    lastRoot = xn;
                    lastMin = xn;
                }
                else
                {
                    resultLabel.Text = $"Метод не сошелся за {maxIterations} итераций!\n" +
                                      $"Последнее приближение: {xn:F8}\n" +
                                      $"f(x) = {fxn:E}\n" +
                                      $"f'(x) = {fpxn:E}\n" +
                                      $"Последнее Δx: {delta:E}";
                }

                // Сохраняем историю итераций для отрисовки
                newtonIterationPoints = iterationPoints;
            }
            catch (FormatException)
            {
                resultLabel.Text = "Ошибка: Проверьте правильность ввода чисел!";
            }
            catch (Exception ex)
            {
                resultLabel.Text = $"Ошибка: {ex.Message}";
            }
        }

        // Поле для хранения истории итераций метода Ньютона
        private List<double> newtonIterationPoints = new List<double>();

        // ОТРИСОВКА ГРАФИКА ДЛЯ МЕТОДА НЬЮТОНА
        private void DrawNewtonGraph(Graphics g, Rectangle drawingArea, string functionStr, string derivativeStr)
        {
            if (string.IsNullOrEmpty(functionStr))
                return;

            try
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                int padding = 40;
                Rectangle graphArea = new Rectangle(
                    drawingArea.Left + padding,
                    drawingArea.Top + padding,
                    drawingArea.Width - 2 * padding,
                    drawingArea.Height - 2 * padding
                );

                DrawGrid(g, graphArea);
                DrawAxes(g, graphArea);
                DrawNewtonFunction(g, graphArea, functionStr);
                DrawNewtonLabels(g, graphArea, drawingArea, functionStr);

                // Если есть история итераций, рисуем касательные
                if (newtonIterationPoints != null && newtonIterationPoints.Count > 0)
                {
                    DrawNewtonTangents(g, graphArea, functionStr, derivativeStr);
                }
            }
            catch (Exception ex)
            {
                g.DrawString($"Ошибка построения: {ex.Message}",
                    new Font("Arial", 10), Brushes.Red, 10, 10);
            }
        }

        // ОТРИСОВКА ГРАФИКА ФУНКЦИИ ДЛЯ МЕТОДА НЬЮТОНА
        private void DrawNewtonFunction(Graphics g, Rectangle graphArea, string functionStr)
        {
            try
            {
                float scaleX = graphArea.Width / 20f;
                float scaleY = graphArea.Height / 20f;

                PointF center = new PointF(
                    graphArea.Left + graphArea.Width / 2,
                    graphArea.Top + graphArea.Height / 2
                );

                using (Pen graphPen = new Pen(Color.Blue, 2))
                {
                    PointF? lastPoint = null;

                    for (int i = 0; i <= graphArea.Width; i++)
                    {
                        double worldX = -10 + (20.0 * i / graphArea.Width);

                        try
                        {
                            double worldY = EvaluateMathExpression(functionStr, worldX);

                            float screenX = center.X + (float)(worldX * scaleX);
                            float screenY = center.Y - (float)(worldY * scaleY);

                            PointF currentPoint = new PointF(screenX, screenY);

                            if (lastPoint.HasValue)
                            {
                                g.DrawLine(graphPen, lastPoint.Value, currentPoint);
                            }

                            lastPoint = currentPoint;
                        }
                        catch
                        {
                            lastPoint = null;
                        }
                    }
                }
            }
            catch { }
        }

        // ОТРИСОВКА КАСАТЕЛЬНЫХ ДЛЯ ИТЕРАЦИЙ МЕТОДА НЬЮТОНА (для минимума)
        private void DrawNewtonTangents(Graphics g, Rectangle graphArea, string functionStr, string derivativeStr)
        {
            if (newtonIterationPoints == null || newtonIterationPoints.Count == 0)
                return;

            float scaleX = graphArea.Width / 20f;
            float scaleY = graphArea.Height / 20f;

            PointF center = new PointF(
                graphArea.Left + graphArea.Width / 2,
                graphArea.Top + graphArea.Height / 2
            );

            Color[] tangentColors = { Color.Red, Color.Green, Color.Purple, Color.Orange };

            for (int i = 0; i < Math.Min(newtonIterationPoints.Count - 1, 4); i++)
            {
                double x0 = newtonIterationPoints[i];

                try
                {
                    double y0 = EvaluateMathExpression(functionStr, x0);

                    // Вычисляем производную
                    double derivative;
                    if (derivativeStr.Contains("f(x+") && derivativeStr.Contains("f(x)"))
                    {
                        double h = 0.0001;
                        double fxh = EvaluateMathExpression(functionStr, x0 + h);
                        derivative = (fxh - y0) / h;
                    }
                    else
                    {
                        derivative = EvaluateMathExpression(derivativeStr, x0);
                    }

                    // Уравнение касательной: y = y0 + derivative*(x - x0)
                    double x1 = x0 - 2;
                    double x2 = x0 + 2;

                    double y1 = y0 + derivative * (x1 - x0);
                    double y2 = y0 + derivative * (x2 - x0);

                    // Преобразуем в экранные координаты
                    float screenX1 = center.X + (float)(x1 * scaleX);
                    float screenY1 = center.Y - (float)(y1 * scaleY);
                    float screenX2 = center.X + (float)(x2 * scaleX);
                    float screenY2 = center.Y - (float)(y2 * scaleY);

                    // Рисуем касательную
                    using (Pen tangentPen = new Pen(tangentColors[i % tangentColors.Length], 1.5f))
                    {
                        tangentPen.DashStyle = DashStyle.Dash;
                        g.DrawLine(tangentPen, screenX1, screenY1, screenX2, screenY2);
                    }

                    // Рисуем точку на графике
                    float pointX = center.X + (float)(x0 * scaleX);
                    float pointY = center.Y - (float)(y0 * scaleY);

                    g.FillEllipse(Brushes.Red, pointX - 4, pointY - 4, 8, 8);
                    g.DrawEllipse(Pens.DarkRed, pointX - 4, pointY - 4, 8, 8);

                    // Подписываем итерацию
                    g.DrawString($"x{i}", new Font("Arial", 8, FontStyle.Bold),
                        Brushes.DarkRed, pointX + 5, pointY - 10);

                    // Рисуем направление движения к минимуму
                    if (i < newtonIterationPoints.Count - 1)
                    {
                        double nextX = newtonIterationPoints[i + 1];
                        float nextScreenX = center.X + (float)(nextX * scaleX);
                        float nextPointY = center.Y - (float)(EvaluateMathExpression(functionStr, nextX) * scaleY);

                        using (Pen guidePen = new Pen(Color.Gray, 1f))
                        {
                            guidePen.DashStyle = DashStyle.Dot;
                            g.DrawLine(guidePen, pointX, pointY, nextScreenX, nextPointY);
                        }
                    }
                }
                catch { }
            }

            // Рисуем найденную точку минимума/максимума
            if (lastRoot != 0)
            {
                try
                {
                    float pointX = center.X + (float)(lastRoot * scaleX);
                    float pointY = center.Y - (float)(EvaluateMathExpression(currentFunction, lastRoot) * scaleY);

                    // Определяем цвет в зависимости от типа точки
                    Brush pointBrush = Brushes.Green; // по умолчанию зеленый
                    Pen pointPen = Pens.DarkGreen;
                    string pointLabel = "Минимум";

                    // Проверяем вторую производную
                    try
                    {
                        string secondDeriv = CalculateSecondDerivative(currentFunction);
                        double fpp = EvaluateMathExpression(secondDeriv, lastRoot);

                        if (fpp < 0)
                        {
                            pointBrush = Brushes.Red;
                            pointPen = Pens.DarkRed;
                            pointLabel = "Максимум";
                        }
                        else if (Math.Abs(fpp) < 0.001)
                        {
                            pointBrush = Brushes.Orange;
                            pointPen = Pens.DarkOrange;
                            pointLabel = "Стац. точка";
                        }
                    }
                    catch { }

                    // Рисуем точку
                    g.FillEllipse(pointBrush, pointX - 5, pointY - 5, 10, 10);
                    g.DrawEllipse(pointPen, pointX - 5, pointY - 5, 10, 10);

                    // Подписываем
                    g.DrawString($"{pointLabel}: {lastRoot:F4}",
                        new Font("Arial", 9, FontStyle.Bold), Brushes.DarkGreen,
                        pointX + 5, pointY - 15);

                    // Рисуем горизонтальную линию через точку (касательная в стационарной точке)
                    using (Pen stationPen = new Pen(Color.Gray, 1f) { DashStyle = DashStyle.Dash })
                    {
                        g.DrawLine(stationPen, graphArea.Left, pointY, graphArea.Right, pointY);
                    }
                }
                catch { }
            }
        }

        // ПОДПИСИ ДЛЯ ГРАФИКА МЕТОДА НЬЮТОНА (для минимума)
        private void DrawNewtonLabels(Graphics g, Rectangle graphArea, Rectangle drawingArea, string functionStr)
        {
            Font labelFont = new Font("Arial", 9);
            Font titleFont = new Font("Arial", 10, FontStyle.Bold);

            PointF center = new PointF(
                graphArea.Left + graphArea.Width / 2,
                graphArea.Top + graphArea.Height / 2
            );

            // Подписи осей
            g.DrawString("X", titleFont, Brushes.Black, graphArea.Right - 15, center.Y - 20);
            g.DrawString("Y", titleFont, Brushes.Black, center.X + 10, graphArea.Top);

            // Заголовок
            if (!string.IsNullOrEmpty(functionStr))
            {
                string title = $"Метод Ньютона для минимума: f(x) = {functionStr}"; // ИЗМЕНИЛИ
                SizeF titleSize = g.MeasureString(title, titleFont);
                g.DrawString(title, titleFont, Brushes.DarkBlue,
                    drawingArea.Left + 10, drawingArea.Top + 10);
            }

            // Легенда
            string legend = "Обозначения:  ● - итерации  --- - касательные  ● - найденный минимум/максимум"; // ИЗМЕНИЛИ
            g.DrawString(legend, new Font("Arial", 8), Brushes.DarkGray,
                graphArea.Left, graphArea.Bottom + 5);
        }

        //**********************************************************************************************| СОРТИРОВКИ |*****************************************************************************************//

        // ИНТЕРФЕЙС СОРТИРОВКИ
        private void InitializeSortingControls()
        {
            // Очищаем панель
            panel1.Controls.Clear();

            // Создаем массив для хранения отсортированных массивов
            currentArray = new int[0];
            originalArray = new int[0];

            // Элементы управления

            var labelArraySize = new Label { Text = "Размер массива:", Location = new Point(10, 10), AutoSize = true };
            var textBoxArraySize = new System.Windows.Forms.TextBox
            {
                Location = new Point(220, 10),
                Width = 100,
                Text = "100",
                BackColor = Color.WhiteSmoke
            };

            var labelMinValue = new Label { Text = "Минимальное значение:", Location = new Point(10, 40), AutoSize = true };
            var textBoxMinValue = new System.Windows.Forms.TextBox
            {
                Location = new Point(220, 40),
                Width = 100,
                Text = "-1000",
                BackColor = Color.WhiteSmoke
            };

            var labelMaxValue = new Label { Text = "Максимальное значение:", Location = new Point(10, 70), AutoSize = true };
            var textBoxMaxValue = new System.Windows.Forms.TextBox
            {
                Location = new Point(220, 70),
                Width = 100,
                Text = "1000",
                BackColor = Color.WhiteSmoke
            };

            var labelMaxIterations = new Label { Text = "Макс. итераций (0=без огр.):", Location = new Point(10, 100), AutoSize = true };
            var textBoxMaxIterations = new System.Windows.Forms.TextBox
            {
                Location = new Point(220, 100),
                Width = 100,
                Text = "0",
                BackColor = Color.WhiteSmoke
            };

            // Кнопки
            var btnGenerateArray = new System.Windows.Forms.Button
            {
                Text = "Сгенерировать массив",
                Location = new Point(10, 130),
                BackColor = Color.LightBlue,
                Width = 150,
                Height = 25
            };

            var btnShowArray = new System.Windows.Forms.Button
            {
                Text = "Показать массив",
                Location = new Point(170, 130),
                BackColor = Color.LightGreen,
                Width = 150,
                Height = 25
            };

            var btnSelectAll = new System.Windows.Forms.Button
            {
                Text = "Выбрать все",
                Location = new Point(10, 190),
                BackColor = Color.LightYellow,
                Width = 100,
                Height = 25
            };

            var btnDeselectAll = new System.Windows.Forms.Button
            {
                Text = "Снять все",
                Location = new Point(120, 190),
                BackColor = Color.LightCoral,
                Width = 100,
                Height = 25
            };

            var btnRunSorts = new System.Windows.Forms.Button
            {
                Text = "Запустить выбранные",
                Location = new Point(10, 160),
                BackColor = Color.MediumSeaGreen,
                Width = 150,
                Height = 25,
            };

            var btnCompareAll = new System.Windows.Forms.Button
            {
                Text = "Сравнить",
                Location = new Point(170, 160),
                BackColor = Color.MediumPurple,
                Width = 150,
                Height = 25,
            };

            // Чекбоксы для выбора сортировок
            int startY = 250;
            int checkboxSpacing = 30;

            var chkBubble = new CheckBox { Text = "Пузырьковая сортировка", Location = new Point(10, 20), Width = 200, Checked = true };
            var lblBubbleTime = new Label { Text = "0 мс", Location = new Point(220, 20), Width = 100, ForeColor = Color.DarkGreen, TextAlign = ContentAlignment.MiddleLeft };

            var chkShaker = new CheckBox { Text = "Шейкерная сортировка", Location = new Point(10, 50), Width = 200, Checked = true };
            var lblShakerTime = new Label { Text = "0 мс", Location = new Point(220, 50), Width = 100, ForeColor = Color.DarkGreen, TextAlign = ContentAlignment.MiddleLeft };

            var chkInsertion = new CheckBox { Text = "Сортировка вставками", Location = new Point(10, 80), Width = 200, Checked = true };
            var lblInsertionTime = new Label { Text = "0 мс", Location = new Point(220, 80), Width = 100, ForeColor = Color.DarkGreen, TextAlign = ContentAlignment.MiddleLeft };

            var chkQuick = new CheckBox { Text = "Быстрая сортировка", Location = new Point(10, 110), Width = 200, Checked = true };
            var lblQuickTime = new Label { Text = "0 мс", Location = new Point(220, 110), Width = 100, ForeColor = Color.DarkGreen, TextAlign = ContentAlignment.MiddleLeft };

            var chkBogo = new CheckBox { Text = "Болотная сортировка", Location = new Point(10, 140), Width = 200, Checked = false };
            var lblBogoTime = new Label { Text = "0 мс", Location = new Point(220, 140), Width = 100, ForeColor = Color.DarkRed, TextAlign = ContentAlignment.MiddleLeft };

            // Группировка для чекбоксов
            var groupBoxSorts = new GroupBox
            {
                Text = "Выбор алгоритмов сортировки",
                Location = new Point(10, startY - 25),
                Size = new Size(330, checkboxSpacing * 5 + 30)
            };

            // Панель для отображения массива
            var arrayPanel = new Panel
            {
                Location = new Point(350, 40),
                Size = new Size(420, 350),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                BackColor = Color.White
            };

            var labelArray = new Label
            {
                Text = "Массив:",
                Location = new Point(350, 10),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            // TextBox для вывода результатов сравнения
            var resultTextBox = new System.Windows.Forms.TextBox
            {
                Location = new Point(10, startY + checkboxSpacing * 5 + 10),
                Size = new Size(760, 70),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9),
                BackColor = Color.Lavender
            };

            // Статистика
            var statsLabel = new Label
            {
                Location = new Point(10, startY + checkboxSpacing * 5 + 120),
                Size = new Size(790, 50),
                Text = "Статистика: Ожидание генерации массива...",
                Font = new Font("Arial", 9),
                ForeColor = Color.DarkSlateGray
            };

            // Добавляем чекбоксы в группу
            groupBoxSorts.Controls.Add(chkBubble);
            groupBoxSorts.Controls.Add(lblBubbleTime);
            groupBoxSorts.Controls.Add(chkShaker);
            groupBoxSorts.Controls.Add(lblShakerTime);
            groupBoxSorts.Controls.Add(chkInsertion);
            groupBoxSorts.Controls.Add(lblInsertionTime);
            groupBoxSorts.Controls.Add(chkQuick);
            groupBoxSorts.Controls.Add(lblQuickTime);
            groupBoxSorts.Controls.Add(chkBogo);
            groupBoxSorts.Controls.Add(lblBogoTime);

            // Обработчики событий
            btnGenerateArray.Click += (s, e) =>
            {
                GenerateRandomArray(
                    textBoxArraySize.Text,
                    textBoxMinValue.Text,
                    textBoxMaxValue.Text,
                    arrayPanel,
                    statsLabel
                );
            };

            btnShowArray.Click += (s, e) =>
            {
                ShowArrayInPanel(arrayPanel, currentArray.Length > 0 ? currentArray : originalArray);
            };

            btnSelectAll.Click += (s, e) =>
            {
                chkBubble.Checked = true;
                chkShaker.Checked = true;
                chkInsertion.Checked = true;
                chkQuick.Checked = true;
                chkBogo.Checked = false; // Болотную оставляем выключенной по умолчанию
            };

            btnDeselectAll.Click += (s, e) =>
            {
                chkBubble.Checked = false;
                chkShaker.Checked = false;
                chkInsertion.Checked = false;
                chkQuick.Checked = false;
                chkBogo.Checked = false;
            };

            btnRunSorts.Click += (s, e) =>
            {
                RunSelectedSorts(
                    chkBubble, lblBubbleTime,
                    chkShaker, lblShakerTime,
                    chkInsertion, lblInsertionTime,
                    chkQuick, lblQuickTime,
                    chkBogo, lblBogoTime,
                    textBoxMaxIterations.Text,
                    resultTextBox,
                    statsLabel,
                    arrayPanel
                );
            };

            btnCompareAll.Click += (s, e) =>
            {
                CompareAllAlgorithms(
                    textBoxMaxIterations.Text,
                    resultTextBox,
                    statsLabel
                );
            };

            // Предупреждение для болотной сортировки
            chkBogo.CheckedChanged += (s, e) =>
            {
                if (chkBogo.Checked)
                {
                    if (MessageBox.Show("ВНИМАНИЕ!\nБолотная сортировка имеет факториальную сложность O(n!).\nДля массива из 10 элементов это 3,6 млн итераций.\nДля 11 элементов - 39,9 млн итераций.\nПродолжить?",
                        "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        chkBogo.Checked = false;
                    }
                }
            };

            // Добавляем элементы на панель
            panel1.Controls.AddRange(new Control[]
            {
        labelArraySize, textBoxArraySize,
        labelMinValue, textBoxMinValue,
        labelMaxValue, textBoxMaxValue,
        labelMaxIterations, textBoxMaxIterations,
        btnGenerateArray, btnShowArray, btnSelectAll, btnDeselectAll,
        btnRunSorts, btnCompareAll,
        groupBoxSorts,
        labelArray, arrayPanel,
        resultTextBox,
        statsLabel
            });

            // Генерируем начальный массив
            GenerateRandomArray("100", "-1000", "1000", arrayPanel, statsLabel);
        }

        // Поля для хранения массивов
        private int[] currentArray;
        private int[] originalArray;

        // ГЕНЕРАЦИЯ СЛУЧАЙНОГО МАССИВА
        private void GenerateRandomArray(string sizeStr, string minStr, string maxStr, Panel arrayPanel, Label statsLabel)
        {
            try
            {
                int size = int.Parse(sizeStr);
                int min = int.Parse(minStr);
                int max = int.Parse(maxStr);

                if (size <= 0)
                {
                    statsLabel.Text = "Ошибка: Размер массива должен быть больше 0!";
                    return;
                }

                if (min >= max)
                {
                    statsLabel.Text = "Ошибка: Минимальное значение должно быть меньше максимального!";
                    return;
                }

                if (size > 10000)
                {
                    if (MessageBox.Show($"Вы хотите создать массив из {size} элементов.\nЭто может занять много времени. Продолжить?",
                        "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        return;
                    }
                }

                Random rand = new Random();
                originalArray = new int[size];

                for (int i = 0; i < size; i++)
                {
                    originalArray[i] = rand.Next(min, max + 1);
                }

                currentArray = (int[])originalArray.Clone();

                ShowArrayInPanel(arrayPanel, originalArray);

                statsLabel.Text = $"Массив сгенерирован: {size} элементов, значения от {min} до {max}";
                statsLabel.ForeColor = Color.DarkGreen;
            }
            catch (FormatException)
            {
                statsLabel.Text = "Ошибка: Проверьте правильность ввода чисел!";
                statsLabel.ForeColor = Color.Red;
            }
            catch (Exception ex)
            {
                statsLabel.Text = $"Ошибка: {ex.Message}";
                statsLabel.ForeColor = Color.Red;
            }
        }

        // ПОКАЗ МАССИВА В ПАНЕЛИ
        private void ShowArrayInPanel(Panel panel, int[] array)
        {
            panel.Controls.Clear();

            if (array == null || array.Length == 0)
            {
                var label = new Label { Text = "Массив пуст", Location = new Point(10, 10) };
                panel.Controls.Add(label);
                return;
            }

            int x = 5;
            int y = 5;
            int maxWidth = panel.Width - 20;
            int currentWidth = 0;

            for (int i = 0; i < Math.Min(array.Length, 100); i++) // Показываем только первые 100 элементов
            {
                string num = array[i].ToString();
                var label = new Label
                {
                    Text = num,
                    Location = new Point(x, y),
                    AutoSize = true,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.LightYellow,
                    Padding = new Padding(2),
                    Margin = new Padding(1)
                };

                panel.Controls.Add(label);

                x += label.Width + 5;
                currentWidth += label.Width + 5;

                if (currentWidth > maxWidth)
                {
                    x = 5;
                    y += 25;
                    currentWidth = 0;
                }
            }

            if (array.Length > 100)
            {
                var label = new Label
                {
                    Text = $"... и еще {array.Length - 100} элементов",
                    Location = new Point(10, y + 10),
                    AutoSize = true,
                    Font = new Font("Arial", 9, FontStyle.Italic),
                    ForeColor = Color.Gray
                };
                panel.Controls.Add(label);
            }
        }

        // ЗАПУСК ВЫБРАННЫХ СОРТИРОВОК
        private void RunSelectedSorts(CheckBox chkBubble, Label lblBubbleTime, CheckBox chkShaker, Label lblShakerTime, CheckBox chkInsertion, Label lblInsertionTime,
            CheckBox chkQuick, Label lblQuickTime, CheckBox chkBogo, Label lblBogoTime, string maxIterationsStr, System.Windows.Forms.TextBox resultTextBox,
            Label statsLabel, Panel arrayPanel)
        {
            if (originalArray == null || originalArray.Length == 0)
            {
                statsLabel.Text = "Ошибка: Сначала сгенерируйте массив!";
                statsLabel.ForeColor = Color.Red;
                return;
            }

            try
            {
                int maxIterations = int.Parse(maxIterationsStr);

                // Сбрасываем метки времени
                lblBubbleTime.Text = "0 мс";
                lblShakerTime.Text = "0 мс";
                lblInsertionTime.Text = "0 мс";
                lblQuickTime.Text = "0 мс";
                lblBogoTime.Text = "0 мс";

                List<string> results = new List<string>();
                results.Add($"=== РЕЗУЛЬТАТЫ СОРТИРОВКИ ({DateTime.Now:HH:mm:ss}) ===\n");
                results.Add($"Размер массива: {originalArray.Length} элементов\n");

                Stopwatch sw = new Stopwatch();

                // Пузырьковая сортировка
                if (chkBubble.Checked)
                {
                    int[] arr = (int[])originalArray.Clone();
                    sw.Restart();
                    BubbleSort(arr, maxIterations);
                    sw.Stop();
                    lblBubbleTime.Text = $"{sw.ElapsedMilliseconds} мс";
                    results.Add($"Пузырьковая: {sw.ElapsedMilliseconds} мс");
                }

                // Шейкерная сортировка
                if (chkShaker.Checked)
                {
                    int[] arr = (int[])originalArray.Clone();
                    sw.Restart();
                    ShakerSort(arr, maxIterations);
                    sw.Stop();
                    lblShakerTime.Text = $"{sw.ElapsedMilliseconds} мс";
                    results.Add($"Шейкерная: {sw.ElapsedMilliseconds} мс");
                }

                // Сортировка вставками
                if (chkInsertion.Checked)
                {
                    int[] arr = (int[])originalArray.Clone();
                    sw.Restart();
                    InsertionSort(arr, maxIterations);
                    sw.Stop();
                    lblInsertionTime.Text = $"{sw.ElapsedMilliseconds} мс";
                    results.Add($"Вставками: {sw.ElapsedMilliseconds} мс");
                }

                // Быстрая сортировка
                if (chkQuick.Checked)
                {
                    int[] arr = (int[])originalArray.Clone();
                    sw.Restart();
                    QuickSort(arr, 0, arr.Length - 1, maxIterations);
                    sw.Stop();
                    lblQuickTime.Text = $"{sw.ElapsedMilliseconds} мс";
                    results.Add($"Быстрая: {sw.ElapsedMilliseconds} мс");

                    // Сохраняем отсортированный массив для отображения
                    currentArray = arr;
                }

                // Болотная сортировка (только для маленьких массивов)
                if (chkBogo.Checked)
                {
                    if (originalArray.Length > 25)
                    {
                        results.Add($"Болотная: НЕ ВЫПОЛНЕНА (слишком большой массив >25)");
                        lblBogoTime.Text = ">25 эл.";
                    }
                    else
                    {
                        int[] arr = (int[])originalArray.Clone();
                        sw.Restart();
                        int iterations = BogoSort(arr, maxIterations);
                        sw.Stop();
                        lblBogoTime.Text = $"{sw.ElapsedMilliseconds} мс ({iterations} итер.)";
                        results.Add($"Болотная: {sw.ElapsedMilliseconds} мс ({iterations} итераций)");
                    }
                }

                // Выводим результаты
                resultTextBox.Text = string.Join("\n", results);

                // Показываем отсортированный массив
                if (currentArray != null && currentArray.Length > 0)
                {
                    ShowArrayInPanel(arrayPanel, currentArray);
                }

                statsLabel.Text = $"Сортировки завершены. Время указано в миллисекундах.";
                statsLabel.ForeColor = Color.DarkGreen;
            }
            catch (Exception ex)
            {
                statsLabel.Text = $"Ошибка: {ex.Message}";
                statsLabel.ForeColor = Color.Red;
            }
        }

        // СРАВНЕНИЕ ВСЕХ АЛГОРИТМОВ
        private void CompareAllAlgorithms(string maxIterationsStr, System.Windows.Forms.TextBox resultTextBox, Label statsLabel)
        {
            if (originalArray == null || originalArray.Length == 0)
            {
                statsLabel.Text = "Ошибка: Сначала сгенерируйте массив!";
                statsLabel.ForeColor = Color.Red;
                return;
            }

            try
            {
                int maxIterations = int.Parse(maxIterationsStr);

                List<AlgorithmResult> results = new List<AlgorithmResult>();
                Stopwatch sw = new Stopwatch();

                // Пузырьковая
                int[] arr = (int[])originalArray.Clone();
                sw.Restart();
                BubbleSort(arr, maxIterations);
                sw.Stop();
                results.Add(new AlgorithmResult("Пузырьковая", sw.ElapsedTicks, sw.ElapsedMilliseconds));

                // Шейкерная
                arr = (int[])originalArray.Clone();
                sw.Restart();
                ShakerSort(arr, maxIterations);
                sw.Stop();
                results.Add(new AlgorithmResult("Шейкерная", sw.ElapsedTicks, sw.ElapsedMilliseconds));

                // Вставками
                arr = (int[])originalArray.Clone();
                sw.Restart();
                InsertionSort(arr, maxIterations);
                sw.Stop();
                results.Add(new AlgorithmResult("Вставками", sw.ElapsedTicks, sw.ElapsedMilliseconds));

                // Быстрая
                arr = (int[])originalArray.Clone();
                sw.Restart();
                QuickSort(arr, 0, arr.Length - 1, maxIterations);
                sw.Stop();
                results.Add(new AlgorithmResult("Быстрая", sw.ElapsedTicks, sw.ElapsedMilliseconds));

                // Болотная (только для маленьких массивов)
                if (originalArray.Length <= 10)
                {
                    arr = (int[])originalArray.Clone();
                    sw.Restart();
                    int iterations = BogoSort(arr, maxIterations);
                    sw.Stop();
                    results.Add(new AlgorithmResult("Болотная", sw.ElapsedTicks, sw.ElapsedMilliseconds, iterations));
                }

                // Сортируем результаты по времени
                results.Sort((a, b) => a.Milliseconds.CompareTo(b.Milliseconds));

                // Формируем отчет
                StringBuilder report = new StringBuilder();
                report.AppendLine("=== СРАВНЕНИЕ АЛГОРИТМОВ СОРТИРОВКИ ===");
                report.AppendLine($"Размер массива: {originalArray.Length} элементов");
                report.AppendLine($"Дата теста: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                report.AppendLine();
                report.AppendLine("Рейтинг алгоритмов (от быстрого к медленному):");
                report.AppendLine();

                for (int i = 0; i < results.Count; i++)
                {
                    var result = results[i];
                    report.AppendLine($"{i + 1}. {result.Name}");
                    report.AppendLine($"   Время: {result.Milliseconds} мс ({result.Ticks} тиков)");
                    if (result.Iterations > 0)
                        report.AppendLine($"   Итераций: {result.Iterations:N0}");
                    report.AppendLine();
                }

                report.AppendLine("=== ВЫВОДЫ ===");
                report.AppendLine($"Самый быстрый: {results[0].Name} ({results[0].Milliseconds} мс)");
                report.AppendLine($"Самый медленный: {results[results.Count - 1].Name} ({results[results.Count - 1].Milliseconds} мс)");
                report.AppendLine($"Разница: {results[results.Count - 1].Milliseconds / (double)results[0].Milliseconds:F1}x");

                resultTextBox.Text = report.ToString();

                statsLabel.Text = $"Сравнение завершено. Лучший алгоритм: {results[0].Name}";
                statsLabel.ForeColor = Color.DarkGreen;
            }
            catch (Exception ex)
            {
                statsLabel.Text = $"Ошибка при сравнении: {ex.Message}";
                statsLabel.ForeColor = Color.Red;
            }
        }

        // Класс для хранения результатов алгоритма
        private class AlgorithmResult
        {
            public string Name { get; }
            public long Ticks { get; }
            public long Milliseconds { get; }
            public long Iterations { get; }

            public AlgorithmResult(string name, long ticks, long milliseconds, long iterations = 0)
            {
                Name = name;
                Ticks = ticks;
                Milliseconds = milliseconds;
                Iterations = iterations;
            }
        }

        // 1. ПУЗЫРЬКОВАЯ СОРТИРОВКА
        private void BubbleSort(int[] array, int maxIterations = 0)
        {
            int n = array.Length;
            int iterations = 0;

            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (array[j] > array[j + 1])
                    {
                        // Меняем элементы местами
                        int temp = array[j];
                        array[j] = array[j + 1];
                        array[j + 1] = temp;
                    }

                    iterations++;
                    if (maxIterations > 0 && iterations >= maxIterations)
                        return;
                }
            }
        }

        // 2. ШЕЙКЕРНАЯ СОРТИРОВКА (COCKTAIL SHAKER SORT)
        private void ShakerSort(int[] array, int maxIterations = 0)
        {
            bool swapped = true;
            int start = 0;
            int end = array.Length - 1;
            int iterations = 0;

            while (swapped)
            {
                swapped = false;

                // Проход слева направо
                for (int i = start; i < end; i++)
                {
                    if (array[i] > array[i + 1])
                    {
                        int temp = array[i];
                        array[i] = array[i + 1];
                        array[i + 1] = temp;
                        swapped = true;
                    }

                    iterations++;
                    if (maxIterations > 0 && iterations >= maxIterations)
                        return;
                }

                if (!swapped)
                    break;

                swapped = false;
                end--;

                // Проход справа налево
                for (int i = end - 1; i >= start; i--)
                {
                    if (array[i] > array[i + 1])
                    {
                        int temp = array[i];
                        array[i] = array[i + 1];
                        array[i + 1] = temp;
                        swapped = true;
                    }

                    iterations++;
                    if (maxIterations > 0 && iterations >= maxIterations)
                        return;
                }

                start++;
            }
        }

        // 3. СОРТИРОВКА ВСТАВКАМИ
        private void InsertionSort(int[] array, int maxIterations = 0)
        {
            int iterations = 0;

            for (int i = 1; i < array.Length; i++)
            {
                int key = array[i];
                int j = i - 1;

                while (j >= 0 && array[j] > key)
                {
                    array[j + 1] = array[j];
                    j = j - 1;

                    iterations++;
                    if (maxIterations > 0 && iterations >= maxIterations)
                        return;
                }

                array[j + 1] = key;
                iterations++;
            }
        }

        // 4. БЫСТРАЯ СОРТИРОВКА
        private void QuickSort(int[] array, int low, int high, int maxIterations = 0)
        {
            int quickSortIterations = 0;

            if (low < high)
            {
                int pi = Partition(array, low, high, ref quickSortIterations, maxIterations);

                if (maxIterations > 0 && quickSortIterations >= maxIterations)
                    return;

                QuickSort(array, low, pi - 1, maxIterations);
                QuickSort(array, pi + 1, high, maxIterations);
            }
        }

        private int Partition(int[] array, int low, int high, ref int iterations, int maxIterations)
        {
            int pivot = array[high];
            int i = (low - 1);

            for (int j = low; j < high; j++)
            {
                if (array[j] < pivot)
                {
                    i++;

                    int temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;
                }

                iterations++;
                if (maxIterations > 0 && iterations >= maxIterations)
                    return i + 1;
            }

            int temp1 = array[i + 1];
            array[i + 1] = array[high];
            array[high] = temp1;

            return i + 1;
        }

        // 5. БОЛОТНАЯ СОРТИРОВКА (BOGO SORT)
        private int BogoSort(int[] array, int maxIterations = 0)
        {
            Random rand = new Random();
            int iterations = 0;

            while (!IsSorted(array))
            {
                // Перемешиваем массив случайным образом
                for (int i = 0; i < array.Length; i++)
                {
                    int j = rand.Next(i, array.Length);
                    int temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;
                }

                iterations++;
                if (maxIterations > 0 && iterations >= maxIterations)
                    break;
            }

            return iterations;
        }

        // ПРОВЕРКА ОТСОРТИРОВАННОСТИ МАССИВА
        private bool IsSorted(int[] array)
        {
            for (int i = 0; i < array.Length - 1; i++)
            {
                if (array[i] > array[i + 1])
                    return false;
            }
            return true;
        }

        //***************************************************************************************| ОПРЕДЕЛЕННЫЙ ИНТЕГРАЛЛ |***********************************************************************************//

        // ИНТЕРФЕЙС вычисление определенного интеграла
        // ИНТЕРФЕЙС вычисление определенного интеграла
        private void InitializeDifiniteIntegralControls()
        {
            // Очищаем панель
            panel1.Controls.Clear();

            // Элементы управления

            var labelFunction = new Label { Text = "Функция f(x):", Location = new Point(10, 10) };
            var textBoxFunction = new System.Windows.Forms.TextBox
            {
                Location = new Point(120, 10),
                Width = 160,
                Text = "x^2",
                BackColor = Color.WhiteSmoke
            };

            var labelA = new Label { Text = "Нижний предел a:", Location = new Point(10, 40) };
            var textBoxA = new System.Windows.Forms.TextBox
            {
                Location = new Point(120, 40),
                Width = 160,
                Text = "0",
                BackColor = Color.WhiteSmoke
            };

            var labelB = new Label { Text = "Верхний предел b:", Location = new Point(10, 70) };
            var textBoxB = new System.Windows.Forms.TextBox
            {
                Location = new Point(120, 70),
                Width = 160,
                Text = "2",
                BackColor = Color.WhiteSmoke
            };

            var labelN = new Label { Text = "Количество разбиений n:", Location = new Point(10, 100), AutoSize = true };
            var textBoxN = new System.Windows.Forms.TextBox
            {
                Location = new Point(180, 100),
                Width = 100,
                Text = "1000",
                BackColor = Color.WhiteSmoke
            };

            var labelEpsilon = new Label { Text = "Точность ", Location = new Point(10, 130) };
            var textBoxEpsilon = new System.Windows.Forms.TextBox
            {
                Location = new Point(120, 130),
                Width = 160,
                Text = "0,0001",
                BackColor = Color.WhiteSmoke
            };

            // Группа выбора метода прямоугольников
            var groupBoxRectangles = new GroupBox
            {
                Text = "Метод прямоугольников",
                Location = new Point(5, 160),
                Size = new Size(290, 120),
                BackColor = Color.Lavender
            };

            var chkRectangles = new CheckBox
            {
                Text = "Использовать метод прямоугольников",
                Location = new Point(10, 20),
                Width = 250,
                Checked = true
            };

            var rbLeftRect = new RadioButton
            {
                Text = "Левых прямоугольников",
                Location = new Point(30, 45),
                Width = 180,
                Checked = true
            };

            var rbRightRect = new RadioButton
            {
                Text = "Правых прямоугольников",
                Location = new Point(30, 70),
                Width = 180
            };

            var rbMiddleRect = new RadioButton
            {
                Text = "Средних прямоугольников",
                Location = new Point(30, 95),
                Width = 180
            };

            // Метод трапеций
            var chkTrapezoidal = new CheckBox
            {
                Text = "Метод трапеций",
                Location = new Point(15, 280),
                Width = 150,
                Checked = true
            };

            // Метод Симпсона
            var chkSimpson = new CheckBox
            {
                Text = "Метод Симпсона (парабол)",
                Location = new Point(15, 310),
                Width = 180,
                Checked = true
            };

            var lblSimpsonNote = new Label
            {
                Text = "*требует четное количество разбиений",
                Location = new Point(15, 333),
                Font = new Font("Arial", 8),
                ForeColor = Color.Gray,
                AutoSize = true
            };

            // Кнопки
            var btnCalculate = new System.Windows.Forms.Button
            {
                Text = "Вычислить",
                Location = new Point(15, 360),
                BackColor = Color.MediumSeaGreen,
                ForeColor = Color.White,
                Width = 120,
                Height = 25
            };

            var btnAllMethods = new System.Windows.Forms.Button
            {
                Text = "Сравнить методы",
                Location = new Point(145, 360),
                BackColor = Color.MediumPurple,
                ForeColor = Color.White,
                Width = 140,
                Height = 25
            };

            var btnDrawGraph = new System.Windows.Forms.Button
            {
                Text = "Построить график",
                Location = new Point(15, 390),
                BackColor = Color.LightBlue,
                Width = 120,
                Height = 25
            };

            // Панель для результатов
            var resultPanel = new Panel
            {
                Location = new Point(320, 30),
                Size = new Size(450, 140),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                BackColor = Color.WhiteSmoke
            };

            var resultLabel = new Label
            {
                Text = "Результаты вычислений:",
                Location = new Point(320, 10),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            // Панель для графика
            var graphPanel = new Panel
            {
                Location = new Point(320, 210),
                Size = new Size(450, 260),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            var graphLabel = new Label
            {
                Text = "График интегрируемой функции",
                Location = new Point(320, 190),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            // Точное значение интеграла (для аналитических функций)
            var labelExact = new Label
            {
                Text = "Точное значение (если известно):",
                Location = new Point(10, 420),
                AutoSize = true,
                Font = new Font("Arial", 9, FontStyle.Italic)
            };

            var textBoxExact = new System.Windows.Forms.TextBox
            {
                Location = new Point(220, 419),
                Width = 61,
                Text = "",
                BackColor = Color.LightYellow
            };

            // Добавляем элементы в группу прямоугольников
            groupBoxRectangles.Controls.Add(chkRectangles);
            groupBoxRectangles.Controls.Add(rbLeftRect);
            groupBoxRectangles.Controls.Add(rbRightRect);
            groupBoxRectangles.Controls.Add(rbMiddleRect);

            // Обработчики событий
            btnCalculate.Click += (s, e) =>
            {
                // Сохраняем параметры для отрисовки
                lastIntegrationFunction = textBoxFunction.Text;
                lastIntegrationA = double.Parse(textBoxA.Text);
                lastIntegrationB = double.Parse(textBoxB.Text);

                // Определяем выбранный метод
                if (chkRectangles.Checked)
                {
                    if (rbLeftRect.Checked) selectedIntegrationMethod = "Левые прямоугольники";
                    else if (rbRightRect.Checked) selectedIntegrationMethod = "Правые прямоугольники";
                    else if (rbMiddleRect.Checked) selectedIntegrationMethod = "Средние прямоугольники";
                }
                else if (chkTrapezoidal.Checked)
                {
                    selectedIntegrationMethod = "Трапеции";
                }
                else if (chkSimpson.Checked)
                {
                    selectedIntegrationMethod = "Симпсон";
                }
                else
                {
                    selectedIntegrationMethod = "Все методы";
                }

                CalculateIntegral(
                    textBoxFunction.Text,
                    textBoxA.Text,
                    textBoxB.Text,
                    textBoxN.Text,
                    textBoxEpsilon.Text,
                    chkRectangles.Checked,
                    rbLeftRect.Checked,
                    rbRightRect.Checked,
                    rbMiddleRect.Checked,
                    chkTrapezoidal.Checked,
                    chkSimpson.Checked,
                    resultPanel,
                    textBoxExact.Text
                );

                // Обновляем график
                graphPanel.Invalidate();
            };

            btnAllMethods.Click += (s, e) =>
            {
                CalculateAllMethods(
                    textBoxFunction.Text,
                    textBoxA.Text,
                    textBoxB.Text,
                    textBoxN.Text,
                    textBoxEpsilon.Text,
                    resultPanel,
                    textBoxExact.Text
                );
            };

            btnDrawGraph.Click += (s, e) =>
            {
                DrawIntegralGraph(
                    graphPanel,
                    textBoxFunction.Text,
                    textBoxA.Text,
                    textBoxB.Text
                );
            };

            // Автоматическое вычисление точного значения для некоторых функций
            textBoxFunction.TextChanged += (s, e) =>
            {
                UpdateExactValue(textBoxFunction.Text, textBoxA.Text, textBoxB.Text, textBoxExact);
            };

            textBoxA.TextChanged += (s, e) =>
            {
                UpdateExactValue(textBoxFunction.Text, textBoxA.Text, textBoxB.Text, textBoxExact);
            };

            textBoxB.TextChanged += (s, e) =>
            {
                UpdateExactValue(textBoxFunction.Text, textBoxA.Text, textBoxB.Text, textBoxExact);
            };

            // Добавляем элементы на панель
            panel1.Controls.AddRange(new Control[]
            {
        labelFunction, textBoxFunction,
        labelA, textBoxA,
        labelB, textBoxB,
        labelN, textBoxN,
        labelEpsilon, textBoxEpsilon,
        groupBoxRectangles,
        chkTrapezoidal,
        chkSimpson, lblSimpsonNote,
        btnCalculate, btnAllMethods, btnDrawGraph,
        resultLabel, resultPanel,
        graphLabel, graphPanel,
        labelExact, textBoxExact
            });

            // Обновляем точное значение
            UpdateExactValue("x^2", "0", "2", textBoxExact);
        }

        // ВЫЧИСЛЕНИЕ ИНТЕГРАЛА ВЫБРАННЫМИ МЕТОДАМИ
        private void CalculateIntegral(string functionStr, string aStr, string bStr, string nStr, string epsilonStr, bool useRectangles, bool leftRect,
            bool rightRect, bool middleRect, bool useTrapezoidal, bool useSimpson, Panel resultPanel, string exactValueStr)
        {
            resultPanel.Controls.Clear();

            try
            {
                double a = double.Parse(aStr);
                double b = double.Parse(bStr);
                int n = int.Parse(nStr);
                double epsilon = double.Parse(epsilonStr);

                if (a >= b)
                {
                    AddResultLabel(resultPanel, "Ошибка: a должно быть меньше b!", Color.Red);
                    return;
                }

                if (n <= 0)
                {
                    AddResultLabel(resultPanel, "Ошибка: количество разбиений должно быть > 0!", Color.Red);
                    return;
                }

                if (epsilon <= 0)
                {
                    AddResultLabel(resultPanel, "Ошибка: точность должна быть > 0!", Color.Red);
                    return;
                }

                // Проверка для метода Симпсона
                if (useSimpson && n % 2 != 0)
                {
                    AddResultLabel(resultPanel, "Для метода Симпсона n должно быть четным!", Color.Orange);
                    n++; // Делаем четным
                    AddResultLabel(resultPanel, $"n увеличено до {n} для метода Симпсона", Color.DarkOrange);
                }

                List<string> results = new List<string>();
                results.Add($"=== ВЫЧИСЛЕНИЕ ИНТЕГРАЛА ===\n");
                results.Add($"Функция: f(x) = {functionStr}");
                results.Add($"Пределы: [{a}; {b}]");
                results.Add($"Количество разбиений: {n}\n");

                double exactValue = 0;
                bool hasExactValue = false;

                // Пробуем получить точное значение
                if (!string.IsNullOrWhiteSpace(exactValueStr))
                {
                    if (double.TryParse(exactValueStr, out exactValue))
                    {
                        hasExactValue = true;
                        results.Add($"Точное значение: {exactValue:F10}");
                    }
                }

                // Метод прямоугольников
                if (useRectangles)
                {
                    double rectResult = 0;
                    string rectType = "";

                    if (leftRect)
                    {
                        rectResult = RectangleLeftMethod(functionStr, a, b, n);
                        rectType = "левых прямоугольников";
                    }
                    else if (rightRect)
                    {
                        rectResult = RectangleRightMethod(functionStr, a, b, n);
                        rectType = "правых прямоугольников";
                    }
                    else if (middleRect)
                    {
                        rectResult = RectangleMiddleMethod(functionStr, a, b, n);
                        rectType = "средних прямоугольников";
                    }

                    results.Add($"Метод {rectType}: {rectResult:F10}");

                    if (hasExactValue)
                    {
                        double error = Math.Abs(rectResult - exactValue);
                        double relativeError = (error / Math.Abs(exactValue)) * 100;
                        results.Add($"  Погрешность: {error:E4} ({relativeError:F4}%)");
                    }
                }

                // Метод трапеций
                if (useTrapezoidal)
                {
                    double trapResult = TrapezoidalMethod(functionStr, a, b, n);
                    results.Add($"Метод трапеций: {trapResult:F10}");

                    if (hasExactValue)
                    {
                        double error = Math.Abs(trapResult - exactValue);
                        double relativeError = (error / Math.Abs(exactValue)) * 100;
                        results.Add($"  Погрешность: {error:E4} ({relativeError:F4}%)");
                    }
                }

                // Метод Симпсона
                if (useSimpson)
                {
                    double simpResult = SimpsonMethod(functionStr, a, b, n);
                    results.Add($"Метод Симпсона: {simpResult:F10}");

                    if (hasExactValue)
                    {
                        double error = Math.Abs(simpResult - exactValue);
                        double relativeError = (error / Math.Abs(exactValue)) * 100;
                        results.Add($"  Погрешность: {error:E4} ({relativeError:F4}%)");
                    }
                }

                // Вывод результатов
                DisplayResults(resultPanel, results);

                // Если есть точное значение, показываем лучший метод
                if (hasExactValue && (useRectangles || useTrapezoidal || useSimpson))
                {
                    ShowBestMethod(resultPanel, functionStr, a, b, n, exactValue);
                }
            }
            catch (FormatException)
            {
                AddResultLabel(resultPanel, "Ошибка: Проверьте правильность ввода чисел!", Color.Red);
            }
            catch (Exception ex)
            {
                AddResultLabel(resultPanel, $"Ошибка: {ex.Message}", Color.Red);
            }
        }

        // ВЫЧИСЛЕНИЕ ВСЕМИ МЕТОДАМИ
        private void CalculateAllMethods(string functionStr, string aStr, string bStr, string nStr, string epsilonStr, Panel resultPanel, string exactValueStr)
        {
            resultPanel.Controls.Clear();

            try
            {
                double a = double.Parse(aStr);
                double b = double.Parse(bStr);
                int n = int.Parse(nStr);
                double epsilon = double.Parse(epsilonStr);

                if (a >= b)
                {
                    AddResultLabel(resultPanel, "Ошибка: a должно быть меньше b!", Color.Red);
                    return;
                }

                if (n <= 0)
                {
                    AddResultLabel(resultPanel, "Ошибка: количество разбиений должно быть > 0!", Color.Red);
                    return;
                }

                // Для Симпсона делаем n четным
                if (n % 2 != 0) n++;

                List<string> results = new List<string>();
                results.Add($"=== СРАВНЕНИЕ МЕТОДОВ ИНТЕГРИРОВАНИЯ ===\n");
                results.Add($"Функция: f(x) = {functionStr}");
                results.Add($"Пределы: [{a}; {b}]");
                results.Add($"Количество разбиений: {n}\n");

                double exactValue = 0;
                bool hasExactValue = false;

                // Пробуем получить точное значение
                if (!string.IsNullOrWhiteSpace(exactValueStr))
                {
                    if (double.TryParse(exactValueStr, out exactValue))
                    {
                        hasExactValue = true;
                        results.Add($"Точное значение: {exactValue:F15}\n");
                    }
                }

                // Вычисляем всеми методами
                List<MethodResult> methodResults = new List<MethodResult>();

                // Метод левых прямоугольников
                double leftRect = RectangleLeftMethod(functionStr, a, b, n);
                methodResults.Add(new MethodResult("Левых прямоугольников", leftRect));

                // Метод правых прямоугольников
                double rightRect = RectangleRightMethod(functionStr, a, b, n);
                methodResults.Add(new MethodResult("Правых прямоугольников", rightRect));

                // Метод средних прямоугольников
                double middleRect = RectangleMiddleMethod(functionStr, a, b, n);
                methodResults.Add(new MethodResult("Средних прямоугольников", middleRect));

                // Метод трапеций
                double trapezoidal = TrapezoidalMethod(functionStr, a, b, n);
                methodResults.Add(new MethodResult("Трапеций", trapezoidal));

                // Метод Симпсона
                double simpson = SimpsonMethod(functionStr, a, b, n);
                methodResults.Add(new MethodResult("Симпсона", simpson));

                // Добавляем результаты в список
                foreach (var method in methodResults)
                {
                    results.Add($"{method.Name}:");
                    results.Add($"  Значение: {method.Value:F15}");

                    if (hasExactValue)
                    {
                        double error = Math.Abs(method.Value - exactValue);
                        double relativeError = (error / Math.Abs(exactValue)) * 100;
                        method.Error = error;
                        results.Add($"  Абс. погрешность: {error:E6}");
                        results.Add($"  Отн. погрешность: {relativeError:F6}%\n");
                    }
                    else
                    {
                        results.Add("");
                    }
                }

                // Сортируем по точности (если есть точное значение)
                if (hasExactValue)
                {
                    methodResults.Sort((x, y) => x.Error.CompareTo(y.Error));

                    results.Add("\n=== РЕЙТИНГ ТОЧНОСТИ ===");
                    for (int i = 0; i < methodResults.Count; i++)
                    {
                        var method = methodResults[i];
                        results.Add($"{i + 1}. {method.Name}");
                        results.Add($"   Погрешность: {method.Error:E6}");
                    }

                    results.Add($"\nЛучший метод: {methodResults[0].Name}");
                    results.Add($"Худший метод: {methodResults[methodResults.Count - 1].Name}");
                    results.Add($"Разница в точности: {methodResults[methodResults.Count - 1].Error / methodResults[0].Error:F1}x");
                }

                // Выводим результаты
                DisplayResults(resultPanel, results);
            }
            catch (Exception ex)
            {
                AddResultLabel(resultPanel, $"Ошибка: {ex.Message}", Color.Red);
            }
        }

        // Класс для хранения результатов методов
        private class MethodResult
        {
            public string Name { get; }
            public double Value { get; }
            public double Error { get; set; }

            public MethodResult(string name, double value)
            {
                Name = name;
                Value = value;
                Error = 0;
            }
        }

        // 1. МЕТОД ЛЕВЫХ ПРЯМОУГОЛЬНИКОВ
        private double RectangleLeftMethod(string functionStr, double a, double b, int n)
        {
            double h = (b - a) / n;
            double sum = 0;

            for (int i = 0; i < n; i++)
            {
                double x = a + i * h;
                sum += EvaluateMathExpression(functionStr, x);
            }

            return sum * h;
        }

        // 2. МЕТОД ПРАВЫХ ПРЯМОУГОЛЬНИКОВ
        private double RectangleRightMethod(string functionStr, double a, double b, int n)
        {
            double h = (b - a) / n;
            double sum = 0;

            for (int i = 1; i <= n; i++)
            {
                double x = a + i * h;
                sum += EvaluateMathExpression(functionStr, x);
            }

            return sum * h;
        }

        // 3. МЕТОД СРЕДНИХ ПРЯМОУГОЛЬНИКОВ
        private double RectangleMiddleMethod(string functionStr, double a, double b, int n)
        {
            double h = (b - a) / n;
            double sum = 0;

            for (int i = 0; i < n; i++)
            {
                double x = a + (i + 0.5) * h;
                sum += EvaluateMathExpression(functionStr, x);
            }

            return sum * h;
        }

        // 4. МЕТОД ТРАПЕЦИЙ
        private double TrapezoidalMethod(string functionStr, double a, double b, int n)
        {
            double h = (b - a) / n;
            double sum = (EvaluateMathExpression(functionStr, a) + EvaluateMathExpression(functionStr, b)) / 2;

            for (int i = 1; i < n; i++)
            {
                double x = a + i * h;
                sum += EvaluateMathExpression(functionStr, x);
            }

            return sum * h;
        }

        // 5. МЕТОД СИМПСОНА (ПАРАБОЛ)
        private double SimpsonMethod(string functionStr, double a, double b, int n)
        {
            if (n % 2 != 0) n++; // Делаем четным

            double h = (b - a) / n;
            double sum = EvaluateMathExpression(functionStr, a) + EvaluateMathExpression(functionStr, b);

            // Сумма для нечетных индексов
            for (int i = 1; i < n; i += 2)
            {
                double x = a + i * h;
                sum += 4 * EvaluateMathExpression(functionStr, x);
            }

            // Сумма для четных индексов
            for (int i = 2; i < n; i += 2)
            {
                double x = a + i * h;
                sum += 2 * EvaluateMathExpression(functionStr, x);
            }

            return sum * h / 3;
        }

        // ОТОБРАЖЕНИЕ РЕЗУЛЬТАТОВ
        private void DisplayResults(Panel panel, List<string> results)
        {
            panel.Controls.Clear();

            int y = 10;
            foreach (string line in results)
            {
                var label = new Label
                {
                    Text = line,
                    Location = new Point(10, y),
                    AutoSize = true,
                    Font = new Font("Consolas", 9),
                    ForeColor = Color.Black
                };

                panel.Controls.Add(label);
                y += 20;
            }
        }

        private void AddResultLabel(Panel panel, string text, Color color)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Arial", 10),
                ForeColor = color
            };

            panel.Controls.Add(label);
        }

        // ПОКАЗ ЛУЧШЕГО МЕТОДА
        private void ShowBestMethod(Panel panel, string functionStr, double a, double b, int n, double exactValue)
        {
            // Вычисляем всеми методами
            double[] results = new double[5];
            string[] names = new string[5]
            {
        "Левых прямоугольников",
        "Правых прямоугольников",
        "Средних прямоугольников",
        "Трапеций",
        "Симпсона"
            };

            results[0] = RectangleLeftMethod(functionStr, a, b, n);
            results[1] = RectangleRightMethod(functionStr, a, b, n);
            results[2] = RectangleMiddleMethod(functionStr, a, b, n);
            results[3] = TrapezoidalMethod(functionStr, a, b, n);
            results[4] = SimpsonMethod(functionStr, a, b, n);

            // Находим лучший метод (наименьшая погрешность)
            int bestIndex = 0;
            double bestError = Math.Abs(results[0] - exactValue);

            for (int i = 1; i < results.Length; i++)
            {
                double error = Math.Abs(results[i] - exactValue);
                if (error < bestError)
                {
                    bestError = error;
                    bestIndex = i;
                }
            }

            var bestLabel = new Label
            {
                Text = $"\n✓ Лучший метод: {names[bestIndex]}\n   Погрешность: {bestError:E6}",
                Location = new Point(10, panel.Controls.Count * 20 + 20),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkGreen
            };

            panel.Controls.Add(bestLabel);
        }

        // ПОСТРОЕНИЕ ГРАФИКА ИНТЕГРАЛА (обновленный)
        private void DrawIntegralGraph(Panel panel, string functionStr, string aStr, string bStr)
        {
            try
            {
                double a = double.Parse(aStr);
                double b = double.Parse(bStr);

                if (a >= b)
                {
                    a = 0;
                    b = 2;
                }

                // Создаем новую лямбду для отрисовки, чтобы избежать накопления обработчиков
                panel.Paint -= (sender, e) => { };
                panel.Paint += (sender, e) =>
                {
                    DrawIntegralFunction(e.Graphics, panel.ClientRectangle, functionStr, a, b);
                };

                panel.Invalidate(); // Перерисовываем
            }
            catch
            {
                // В случае ошибки рисуем сообщение
                panel.Paint += (sender, e) =>
                {
                    e.Graphics.Clear(Color.White);
                    e.Graphics.DrawString("Ошибка в параметрах интеграла!",
                        new Font("Arial", 12), Brushes.Red, 10, 10);
                };
                panel.Invalidate();
            }
        }

        // ОТРИСОВКА ГРАФИКА ИНТЕГРИРУЕМОЙ ФУНКЦИИ (с правильными осями)
        private void DrawIntegralFunction(Graphics g, Rectangle drawingArea, string functionStr, double a, double b)
        {
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // Отступы от краев
            int padding = 40;
            Rectangle graphArea = new Rectangle(
                drawingArea.Left + padding,
                drawingArea.Top + padding,
                drawingArea.Width - 2 * padding,
                drawingArea.Height - 2 * padding
            );

            // Вычисляем диапазон значений функции на интервале [a, b]
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            int samples = 200;

            for (int i = 0; i <= samples; i++)
            {
                double x = a + (b - a) * i / samples;
                try
                {
                    double y = EvaluateMathExpression(functionStr, x);
                    if (!double.IsInfinity(y) && !double.IsNaN(y))
                    {
                        minY = Math.Min(minY, y);
                        maxY = Math.Max(maxY, y);
                    }
                }
                catch { }
            }

            // Если функция не определена на всем интервале, используем значения по умолчанию
            if (minY == double.MaxValue || maxY == double.MinValue)
            {
                minY = -1;
                maxY = 1;
            }

            // Добавляем немного места сверху и снизу (10%)
            double rangeY = maxY - minY;
            if (rangeY < 0.1) rangeY = 1; // Минимальный диапазон
            minY -= rangeY * 0.1;
            maxY += rangeY * 0.1;

            // Масштаб для преобразования мировых координат в экранные
            float scaleX = graphArea.Width / (float)(b - a);
            float scaleY = graphArea.Height / (float)(maxY - minY);

            // Начало координат (левая нижняя точка графика)
            PointF origin = new PointF(
                graphArea.Left,
                graphArea.Bottom
            );

            // Рисуем сетку, привязанную к интервалу [a, b]
            DrawIntegralGrid(g, graphArea, a, b, minY, maxY, origin, scaleX, scaleY);

            // Рисуем оси координат
            DrawIntegralAxes(g, graphArea, a, b, minY, maxY, origin, scaleX, scaleY);

            // Рисуем функцию
            using (Pen graphPen = new Pen(Color.Blue, 2))
            {
                List<PointF> functionPoints = new List<PointF>();
                PointF? lastPoint = null;

                for (int i = 0; i <= graphArea.Width; i++)
                {
                    double x = a + (b - a) * i / graphArea.Width;

                    try
                    {
                        double y = EvaluateMathExpression(functionStr, x);

                        // Ограничиваем слишком большие значения для отображения
                        if (double.IsInfinity(y) || double.IsNaN(y) || Math.Abs(y) > Math.Abs(maxY) * 10)
                        {
                            lastPoint = null;
                            continue;
                        }

                        float screenX = origin.X + (float)((x - a) * scaleX);
                        float screenY = origin.Y - (float)((y - minY) * scaleY);

                        PointF currentPoint = new PointF(screenX, screenY);
                        functionPoints.Add(currentPoint);

                        if (lastPoint.HasValue)
                        {
                            g.DrawLine(graphPen, lastPoint.Value, currentPoint);
                        }

                        lastPoint = currentPoint;
                    }
                    catch
                    {
                        lastPoint = null;
                    }
                }
            }

            // Закрашиваем область под кривой (интеграл)
            DrawIntegralArea(g, graphArea, functionStr, a, b, minY, maxY, origin, scaleX, scaleY);

            // Рисуем обозначения методов интегрирования (если они были вычислены)
            DrawIntegrationMethods(g, graphArea, a, b, minY, maxY, origin, scaleX, scaleY);

            // Подписи и легенда
            DrawIntegralLabels(g, graphArea, drawingArea, functionStr, a, b);
        }

        // ОТРИСОВКА СЕТКИ ДЛЯ ИНТЕГРАЛА (привязанной к интервалу [a, b])
        private void DrawIntegralGrid(Graphics g, Rectangle graphArea, double a, double b, double minY, double maxY, PointF origin, float scaleX, float scaleY)
        {
            Pen gridPen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dot };
            Font gridFont = new Font("Arial", 8);
            Brush gridBrush = Brushes.Gray;

            // Вертикальные линии сетки (оси X)
            int xDivisions = 10; // Количество делений по X
            for (int i = 0; i <= xDivisions; i++)
            {
                double xValue = a + (b - a) * i / xDivisions;
                float screenX = origin.X + (float)((xValue - a) * scaleX);

                if (screenX >= graphArea.Left && screenX <= graphArea.Right)
                {
                    // Линия сетки
                    g.DrawLine(gridPen, screenX, graphArea.Top, screenX, graphArea.Bottom);

                    // Подпись значения X
                    string label = xValue.ToString("F2");
                    SizeF textSize = g.MeasureString(label, gridFont);
                    g.DrawString(label, gridFont, gridBrush,
                        screenX - textSize.Width / 2, graphArea.Bottom + 5);
                }
            }

            // Горизонтальные линии сетки (оси Y)
            int yDivisions = 8; // Количество делений по Y
            for (int i = 0; i <= yDivisions; i++)
            {
                double yValue = minY + (maxY - minY) * i / yDivisions;
                float screenY = origin.Y - (float)((yValue - minY) * scaleY);

                if (screenY >= graphArea.Top && screenY <= graphArea.Bottom)
                {
                    // Линия сетки
                    g.DrawLine(gridPen, graphArea.Left, screenY, graphArea.Right, screenY);

                    // Подпись значения Y
                    string label = yValue.ToString("F2");
                    SizeF textSize = g.MeasureString(label, gridFont);
                    g.DrawString(label, gridFont, gridBrush,
                        graphArea.Left - textSize.Width - 5, screenY - textSize.Height / 2);
                }
            }
        }

        // ОТРИСОВКА ОСЕЙ КООРДИНАТ ДЛЯ ИНТЕГРАЛА
        private void DrawIntegralAxes(Graphics g, Rectangle graphArea, double a, double b, double minY, double maxY, PointF origin, float scaleX, float scaleY)
        {
            Pen axisPen = new Pen(Color.Black, 2);
            Font axisFont = new Font("Arial", 9, FontStyle.Bold);

            // Ось X (горизонтальная) - рисуем только если 0 в диапазоне Y
            if (minY <= 0 && maxY >= 0)
            {
                float zeroY = origin.Y - (float)((0 - minY) * scaleY);
                g.DrawLine(axisPen, graphArea.Left, zeroY, graphArea.Right, zeroY);

                // Стрелка оси X
                g.DrawLine(axisPen, graphArea.Right - 10, zeroY - 5, graphArea.Right, zeroY);
                g.DrawLine(axisPen, graphArea.Right - 10, zeroY + 5, graphArea.Right, zeroY);

                // Подпись оси X
                g.DrawString("X", axisFont, Brushes.Black, graphArea.Right - 15, zeroY - 20);
            }

            // Ось Y (вертикальная) - рисуем только если 0 в диапазоне X
            if (a <= 0 && b >= 0)
            {
                float zeroX = origin.X + (float)((0 - a) * scaleX);
                g.DrawLine(axisPen, zeroX, graphArea.Top, zeroX, graphArea.Bottom);

                // Стрелка оси Y
                g.DrawLine(axisPen, zeroX - 5, graphArea.Top + 10, zeroX, graphArea.Top);
                g.DrawLine(axisPen, zeroX + 5, graphArea.Top + 10, zeroX, graphArea.Top);

                // Подпись оси Y
                g.DrawString("Y", axisFont, Brushes.Black, zeroX + 10, graphArea.Top);
            }

            // Если оси не пересекаются в пределах графика, рисуем их по границам
            if (!(minY <= 0 && maxY >= 0))
            {
                // Рисуем ось X внизу
                g.DrawLine(axisPen, graphArea.Left, graphArea.Bottom, graphArea.Right, graphArea.Bottom);
                g.DrawString("X", axisFont, Brushes.Black, graphArea.Right - 15, graphArea.Bottom - 20);
            }

            if (!(a <= 0 && b >= 0))
            {
                // Рисуем ось Y слева
                g.DrawLine(axisPen, graphArea.Left, graphArea.Top, graphArea.Left, graphArea.Bottom);
                g.DrawString("Y", axisFont, Brushes.Black, graphArea.Left + 10, graphArea.Top);
            }
        }

        // ОТРИСОВКА ОБЛАСТИ ИНТЕГРАЛА И МЕТОДОВ
        private void DrawIntegralArea(Graphics g, Rectangle graphArea, string functionStr, double a, double b, double minY, double maxY, PointF origin, float scaleX, float scaleY)
        {
            // Основная заливка под кривой
            using (Brush integralBrush = new SolidBrush(Color.FromArgb(50, Color.LightBlue)))
            {
                List<PointF> areaPoints = new List<PointF>();

                // Начинаем с левой нижней точки (a, minY или 0)
                double startY = Math.Max(minY, 0);
                areaPoints.Add(new PointF(
                    origin.X,
                    origin.Y - (float)((startY - minY) * scaleY)
                ));

                // Добавляем точки функции
                int segments = 50; // Количество сегментов для аппроксимации
                for (int i = 0; i <= segments; i++)
                {
                    double x = a + (b - a) * i / segments;
                    try
                    {
                        double y = EvaluateMathExpression(functionStr, x);
                        if (double.IsInfinity(y) || double.IsNaN(y)) continue;

                        float screenX = origin.X + (float)((x - a) * scaleX);
                        float screenY = origin.Y - (float)((y - minY) * scaleY);
                        areaPoints.Add(new PointF(screenX, screenY));
                    }
                    catch { }
                }

                // Заканчиваем правой нижней точкой
                areaPoints.Add(new PointF(
                    origin.X + graphArea.Width,
                    origin.Y - (float)((startY - minY) * scaleY)
                ));

                // Рисуем заполненную область
                if (areaPoints.Count > 2)
                {
                    g.FillPolygon(integralBrush, areaPoints.ToArray());
                }
            }

            // Визуализация методов интегрирования (если данные доступны)
            DrawIntegrationVisualization(g, graphArea, functionStr, a, b, minY, maxY, origin, scaleX, scaleY);
        }

        // ВИЗУАЛИЗАЦИЯ МЕТОДОВ ИНТЕГРИРОВАНИЯ -------------------------------------------------------------------
        private void DrawIntegrationVisualization(Graphics g, Rectangle graphArea, string functionStr,
            double a, double b, double minY, double maxY, PointF origin, float scaleX, float scaleY)
        {
            // Получаем текущие настройки метода из интерфейса
            // (здесь нужно передать информацию о выбранном методе)

            int n = 10; // Количество разбиений для визуализации

            // Метод прямоугольников (левый)
            DrawRectanglesMethod(g, graphArea, functionStr, a, b, minY, maxY, origin, scaleX, scaleY, n, "left");

            // Метод трапеций
            DrawTrapezoidalMethod(g, graphArea, functionStr, a, b, minY, maxY, origin, scaleX, scaleY, n);

            // Метод Симпсона (парабол)
            DrawSimpsonMethod(g, graphArea, functionStr, a, b, minY, maxY, origin, scaleX, scaleY, n);
        }
        // Визуализация метода прямоугольников
        private void DrawRectanglesMethod(Graphics g, Rectangle graphArea, string functionStr,
            double a, double b, double minY, double maxY, PointF origin, float scaleX, float scaleY,
            int n, string type)
        {
            double h = (b - a) / n;
            Pen rectPen = new Pen(Color.FromArgb(150, Color.Green), 1);
            Brush rectBrush = new SolidBrush(Color.FromArgb(30, Color.Green));

            for (int i = 0; i < n; i++)
            {
                double x_left = a + i * h;
                double x_right = x_left + h;

                double x_sample = type == "left" ? x_left :
                                 type == "right" ? x_right :
                                 x_left + h / 2; // средний

                try
                {
                    double y = EvaluateMathExpression(functionStr, x_sample);
                    if (double.IsInfinity(y) || double.IsNaN(y)) continue;

                    float rectLeft = origin.X + (float)((x_left - a) * scaleX);
                    float rectRight = origin.X + (float)((x_right - a) * scaleX);
                    float rectTop = origin.Y - (float)((y - minY) * scaleY);
                    float rectBottom = origin.Y - (float)((0 - minY) * scaleY);

                    if (rectTop > rectBottom)
                    {
                        float temp = rectTop;
                        rectTop = rectBottom;
                        rectBottom = temp;
                    }

                    // Заливка прямоугольника
                    g.FillRectangle(rectBrush, rectLeft, rectTop, rectRight - rectLeft, rectBottom - rectTop);

                    // Контур прямоугольника
                    g.DrawRectangle(rectPen, rectLeft, rectTop, rectRight - rectLeft, rectBottom - rectTop);
                }
                catch { }
            }
        }
        // Визуализация метода трапеций
        private void DrawTrapezoidalMethod(Graphics g, Rectangle graphArea, string functionStr,
            double a, double b, double minY, double maxY, PointF origin, float scaleX, float scaleY, int n)
        {
            double h = (b - a) / n;
            Pen trapPen = new Pen(Color.FromArgb(150, Color.Orange), 1);
            Brush trapBrush = new SolidBrush(Color.FromArgb(30, Color.Orange));

            for (int i = 0; i < n; i++)
            {
                double x1 = a + i * h;
                double x2 = x1 + h;

                try
                {
                    double y1 = EvaluateMathExpression(functionStr, x1);
                    double y2 = EvaluateMathExpression(functionStr, x2);

                    if (double.IsInfinity(y1) || double.IsNaN(y1) ||
                        double.IsInfinity(y2) || double.IsNaN(y2)) continue;

                    float screenX1 = origin.X + (float)((x1 - a) * scaleX);
                    float screenX2 = origin.X + (float)((x2 - a) * scaleX);
                    float screenY1 = origin.Y - (float)((y1 - minY) * scaleY);
                    float screenY2 = origin.Y - (float)((y2 - minY) * scaleY);
                    float screenY0 = origin.Y - (float)((0 - minY) * scaleY);

                    // Создаем полигон для трапеции
                    PointF[] trapezoid = new PointF[4];
                    trapezoid[0] = new PointF(screenX1, screenY0);
                    trapezoid[1] = new PointF(screenX1, screenY1);
                    trapezoid[2] = new PointF(screenX2, screenY2);
                    trapezoid[3] = new PointF(screenX2, screenY0);

                    // Заливка трапеции
                    g.FillPolygon(trapBrush, trapezoid);

                    // Контур трапеции
                    g.DrawPolygon(trapPen, trapezoid);
                }
                catch { }
            }
        }
        // Визуализация метода Симпсона
        private void DrawSimpsonMethod(Graphics g, Rectangle graphArea, string functionStr,
            double a, double b, double minY, double maxY, PointF origin, float scaleX, float scaleY, int n)
        {
            if (n % 2 != 0) n++; // Делаем четным

            double h = (b - a) / n;
            Pen simpPen = new Pen(Color.FromArgb(150, Color.Purple), 1);
            Brush simpBrush = new SolidBrush(Color.FromArgb(30, Color.Purple));

            for (int i = 0; i < n; i += 2)
            {
                double x0 = a + i * h;
                double x1 = x0 + h;
                double x2 = x1 + h;

                try
                {
                    double y0 = EvaluateMathExpression(functionStr, x0);
                    double y1 = EvaluateMathExpression(functionStr, x1);
                    double y2 = EvaluateMathExpression(functionStr, x2);

                    if (double.IsInfinity(y0) || double.IsNaN(y0) ||
                        double.IsInfinity(y1) || double.IsNaN(y1) ||
                        double.IsInfinity(y2) || double.IsNaN(y2)) continue;

                    // Аппроксимируем параболой через три точки
                    List<PointF> parabolaPoints = new List<PointF>();
                    int segments = 20;

                    for (int j = 0; j <= segments; j++)
                    {
                        double t = (double)j / segments;
                        double x = x0 + (x2 - x0) * t;

                        // Квадратичная интерполяция (парабола)
                        double y = y0 * (t - 1) * (t - 2) / 2 - y1 * t * (t - 2) + y2 * t * (t - 1) / 2;

                        float screenX = origin.X + (float)((x - a) * scaleX);
                        float screenY = origin.Y - (float)((y - minY) * scaleY);
                        parabolaPoints.Add(new PointF(screenX, screenY));
                    }

                    // Добавляем точки основания
                    float screenY0 = origin.Y - (float)((0 - minY) * scaleY);
                    parabolaPoints.Add(new PointF(
                        origin.X + (float)((x2 - a) * scaleX), screenY0));
                    parabolaPoints.Add(new PointF(
                        origin.X + (float)((x0 - a) * scaleX), screenY0));

                    // Заливка области под параболой
                    if (parabolaPoints.Count > 2)
                    {
                        g.FillPolygon(simpBrush, parabolaPoints.ToArray());
                        g.DrawPolygon(simpPen, parabolaPoints.ToArray());
                    }
                }
                catch { }
            }
        }
        //--------------------------------------------------------------------------------------------------------

        // ОТРИСОВКА ОБОЗНАЧЕНИЙ МЕТОДОВ ИНТЕГРИРОВАНИЯ
        private void DrawIntegrationMethods(Graphics g, Rectangle graphArea, double a, double b, double minY, double maxY, PointF origin, float scaleX, float scaleY)
        {
            Font legendFont = new Font("Arial", 9);
            int legendY = graphArea.Top;

            // Легенда методов
            string[] methods = {
        "Метод прямоугольников",
        "Метод трапеций",
        "Метод Симпсона"
    };

            Color[] colors = {
        Color.FromArgb(150, Color.Green),
        Color.FromArgb(150, Color.Orange),
        Color.FromArgb(150, Color.Purple)
    };

            for (int i = 0; i < methods.Length; i++)
            {
                // Квадратик-индикатор
                g.FillRectangle(new SolidBrush(colors[i]),
                    graphArea.Left, legendY + i * 20, 15, 15);
                g.DrawRectangle(Pens.Black,
                    graphArea.Left, legendY + i * 20, 15, 15);

                // Текст
                g.DrawString(methods[i], legendFont, Brushes.Black,
                    graphArea.Left + 20, legendY + i * 20);
            }
        }

        // ПОДПИСИ ДЛЯ ГРАФИКА ИНТЕГРАЛА
        private void DrawIntegralLabels(Graphics g, Rectangle graphArea, Rectangle drawingArea, string functionStr, double a, double b)
        {
            Font titleFont = new Font("Arial", 11, FontStyle.Bold);
            Font infoFont = new Font("Arial", 9);

            // Заголовок
            string title = $"Интеграл функции: f(x) = {functionStr}";
            g.DrawString(title, titleFont, Brushes.DarkBlue,
                drawingArea.Left + 10, drawingArea.Top + 5);

            // Информация об интервале
            string intervalInfo = $"Интервал интегрирования: [{a:F2}, {b:F2}]";
            g.DrawString(intervalInfo, infoFont, Brushes.DarkGreen,
                drawingArea.Left + 10, drawingArea.Top + 30);

            // Обозначение интеграла
            string integralSymbol = $"∫f(x)dx ≈ площадь закрашенной области";
            g.DrawString(integralSymbol, infoFont, Brushes.DarkRed,
                drawingArea.Left + 10, drawingArea.Top + 50);

            // Ключевые обозначения
            string keyInfo = "Обозначения: сетка - координаты, цвета - методы интегрирования";
            g.DrawString(keyInfo, new Font("Arial", 8), Brushes.Gray,
                graphArea.Left, graphArea.Bottom + 5);
        }

        // ОБНОВЛЕНИЕ ТОЧНОГО ЗНАЧЕНИЯ ИНТЕГРАЛА
        private void UpdateExactValue(string functionStr, string aStr, string bStr, System.Windows.Forms.TextBox exactTextBox)
        {
            try
            {
                double a = double.Parse(aStr);
                double b = double.Parse(bStr);

                // Для некоторых известных функций вычисляем точный интеграл
                functionStr = functionStr.ToLower().Replace(" ", "");

                double exactValue = 0;
                bool calculated = false;

                if (functionStr == "x")
                {
                    exactValue = (b * b - a * a) / 2;
                    calculated = true;
                }
                else if (functionStr == "x^2" || functionStr == "x*x")
                {
                    exactValue = (b * b * b - a * a * a) / 3;
                    calculated = true;
                }
                else if (functionStr == "x^3")
                {
                    exactValue = (b * b * b * b - a * a * a * a) / 4;
                    calculated = true;
                }
                else if (functionStr == "sin(x)")
                {
                    exactValue = Math.Cos(a) - Math.Cos(b);
                    calculated = true;
                }
                else if (functionStr == "cos(x)")
                {
                    exactValue = Math.Sin(b) - Math.Sin(a);
                    calculated = true;
                }
                else if (functionStr == "e^x" || functionStr == "exp(x)")
                {
                    exactValue = Math.Exp(b) - Math.Exp(a);
                    calculated = true;
                }
                else if (functionStr == "1/x")
                {
                    if (a > 0 && b > 0)
                    {
                        exactValue = Math.Log(b) - Math.Log(a);
                        calculated = true;
                    }
                }

                if (calculated)
                {
                    exactTextBox.Text = exactValue.ToString("F10");
                    exactTextBox.BackColor = Color.LightGreen;
                }
                else
                {
                    exactTextBox.Text = "";
                    exactTextBox.BackColor = Color.LightYellow;
                }
            }
            catch
            {
                exactTextBox.Text = "";
                exactTextBox.BackColor = Color.LightYellow;
            }
        }

        // АДАПТИВНЫЙ МЕТОД (автоматический выбор шага для заданной точности)
        private double AdaptiveIntegration(string functionStr, double a, double b, double epsilon,
            Func<string, double, double, int, double> integrationMethod)
        {
            int n = 10; // Начальное количество разбиений
            double prevResult = integrationMethod(functionStr, a, b, n);
            double currentResult;

            do
            {
                n *= 2; // Удваиваем количество разбиений
                currentResult = integrationMethod(functionStr, a, b, n);

                if (Math.Abs(currentResult - prevResult) < epsilon)
                    break;

                prevResult = currentResult;

                // Защита от бесконечного цикла
                if (n > 1000000) break;

            } while (true);

            return currentResult;
        }

        //****************************************************************************************| МЕТОД вычисления СЛАУ |***********************************************************************************//

        // ИНТЕРФЕЙС
        private void InitializeSLAU()
        {
            // Очищаем панель
            panel1.Controls.Clear();
        }

        //*************************************************************************************| МЕТОД ПОКООРДИНАТНОГО СПУСКА |*******************************************************************************//

        // ИНТЕРФЕЙС
        private void InitializeCoordinateDescentControls()
        {
            // Очищаем панель
            panel1.Controls.Clear();

            // Переменные для хранения данных
            List<PointF> dataPoints = new List<PointF>();
            string currentDescentFunction = "";

            // Элементы управления
            var labelFunction = new Label
            {
                Text = "Функция f(x,y):",
                Location = new Point(10, 10),
                AutoSize = true
            };

            var textBoxFunction = new System.Windows.Forms.TextBox
            {
                Location = new Point(120, 10),
                Width = 200,
                Text = "x^2 + y^2",
                BackColor = Color.WhiteSmoke
            };

            var labelX0 = new Label
            {
                Text = "Начальная точка x₀:",
                Location = new Point(10, 40),
                AutoSize = true
            };

            var textBoxX0 = new System.Windows.Forms.TextBox
            {
                Location = new Point(120, 40),
                Width = 80,
                Text = "5",
                BackColor = Color.WhiteSmoke
            };

            var labelY0 = new Label
            {
                Text = "y₀:",
                Location = new Point(210, 40),
                AutoSize = true
            };

            var textBoxY0 = new System.Windows.Forms.TextBox
            {
                Location = new Point(230, 40),
                Width = 80,
                Text = "5",
                BackColor = Color.WhiteSmoke
            };

            var labelEpsilon = new Label
            {
                Text = "Точность ε:",
                Location = new Point(10, 70),
                AutoSize = true
            };

            var textBoxEpsilon = new System.Windows.Forms.TextBox
            {
                Location = new Point(120, 70),
                Width = 200,
                Text = "0,001",
                BackColor = Color.WhiteSmoke
            };

            var labelMaxIterations = new Label
            {
                Text = "Макс. итераций:",
                Location = new Point(10, 100),
                AutoSize = true
            };

            var textBoxMaxIterations = new System.Windows.Forms.TextBox
            {
                Location = new Point(120, 100),
                Width = 200,
                Text = "100",
                BackColor = Color.WhiteSmoke
            };

            var labelStepSize = new Label
            {
                Text = "Шаг α (0.1-0.5):",
                Location = new Point(10, 130),
                AutoSize = true
            };

            var textBoxStepSize = new System.Windows.Forms.TextBox
            {
                Location = new Point(120, 130),
                Width = 200,
                Text = "0,1",
                BackColor = Color.WhiteSmoke
            };

            // Методы спуска
            var groupBoxMethod = new GroupBox
            {
                Text = "Метод спуска",
                Location = new Point(10, 160),
                Size = new Size(310, 70),
                BackColor = Color.Lavender
            };

            var rbGradient = new RadioButton
            {
                Text = "Градиентный спуск",
                Location = new Point(10, 20),
                Width = 140,
                Checked = true
            };

            var rbCoordinate = new RadioButton
            {
                Text = "Покоординатный спуск",
                Location = new Point(160, 20),
                Width = 140
            };

            groupBoxMethod.Controls.Add(rbGradient);
            groupBoxMethod.Controls.Add(rbCoordinate);

            // Кнопки
            var btnCalculate = new System.Windows.Forms.Button
            {
                Text = "Найти минимум",
                Location = new Point(10, 240),
                BackColor = Color.MediumSeaGreen,
                ForeColor = Color.White,
                Width = 150,
                Height = 30
            };

            var btnClear = new System.Windows.Forms.Button
            {
                Text = "Очистить",
                Location = new Point(170, 240),
                BackColor = Color.LightCoral,
                Width = 150,
                Height = 30
            };

            var btnExample1 = new System.Windows.Forms.Button
            {
                Text = "Пример 1: x² + y²",
                Location = new Point(10, 280),
                BackColor = Color.LightBlue,
                Width = 150,
                Height = 25
            };

            var btnExample2 = new System.Windows.Forms.Button
            {
                Text = "Пример 2: (x-1)² + (y+2)²",
                Location = new Point(170, 280),
                BackColor = Color.LightBlue,
                Width = 150,
                Height = 25
            };

            var btnExample3 = new System.Windows.Forms.Button
            {
                Text = "Пример 3: Розенброка",
                Location = new Point(10, 310),
                BackColor = Color.LightBlue,
                Width = 150,
                Height = 25
            };

            // Панель для результатов
            var resultPanel = new Panel
            {
                Location = new Point(10, 350),
                Size = new Size(310, 100),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                BackColor = Color.WhiteSmoke
            };

            // Панель для графика 3D
            var graphPanel = new Panel
            {
                Location = new Point(330, 10),
                Size = new Size(440, 440),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            // Обработчики событий для примеров
            btnExample1.Click += (s, e) =>
            {
                textBoxFunction.Text = "x^2 + y^2";
                textBoxX0.Text = "5";
                textBoxY0.Text = "5";
                textBoxStepSize.Text = "0,1";
            };

            btnExample2.Click += (s, e) =>
            {
                textBoxFunction.Text = "(x-1)^2 + (y+2)^2";
                textBoxX0.Text = "-3";
                textBoxY0.Text = "3";
                textBoxStepSize.Text = "0,1";
            };

            btnExample3.Click += (s, e) =>
            {
                textBoxFunction.Text = "100*(y-x^2)^2 + (1-x)^2";
                textBoxX0.Text = "-1,5";
                textBoxY0.Text = "1";
                textBoxStepSize.Text = "0,001";
                textBoxEpsilon.Text = "0,0001";
            };

            // Обработчик вычисления
            btnCalculate.Click += (s, e) =>
            {
                try
                {
                    string functionStr = textBoxFunction.Text;
                    double x0 = double.Parse(textBoxX0.Text);
                    double y0 = double.Parse(textBoxY0.Text);
                    double epsilon = double.Parse(textBoxEpsilon.Text);
                    int maxIterations = int.Parse(textBoxMaxIterations.Text);
                    double stepSize = double.Parse(textBoxStepSize.Text);
                    bool useCoordinateDescent = rbCoordinate.Checked;

                    // Очищаем предыдущие точки
                    dataPoints.Clear();
                    dataPoints.Add(new PointF((float)x0, (float)y0));

                    // Запускаем метод
                    if (useCoordinateDescent)
                    {
                        RunCoordinateDescent(
                            functionStr,
                            x0, y0,
                            epsilon,
                            maxIterations,
                            stepSize,
                            resultPanel,
                            ref dataPoints
                        );
                    }
                    else
                    {
                        RunGradientDescent(
                            functionStr,
                            x0, y0,
                            epsilon,
                            maxIterations,
                            stepSize,
                            resultPanel,
                            ref dataPoints
                        );
                    }

                    // Обновляем график
                    currentDescentFunction = functionStr;
                    graphPanel.Invalidate();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Обработчик очистки
            btnClear.Click += (s, e) =>
            {
                dataPoints.Clear();
                resultPanel.Controls.Clear();
                graphPanel.Invalidate();
            };

            // Обработчик отрисовки графика
            graphPanel.Paint += (sender, e) =>
            {
                DrawDescentGraph(e.Graphics, graphPanel.ClientRectangle,
                    currentDescentFunction, dataPoints);
            };

            // Добавляем элементы на панель
            panel1.Controls.AddRange(new Control[]
            {
        labelFunction, textBoxFunction,
        labelX0, textBoxX0, labelY0, textBoxY0,
        labelEpsilon, textBoxEpsilon,
        labelMaxIterations, textBoxMaxIterations,
        labelStepSize, textBoxStepSize,
        groupBoxMethod,
        btnCalculate, btnClear,
        btnExample1, btnExample2, btnExample3,
        resultPanel,
        graphPanel
            });
        }

        // Метод покоординатного спуска
        private void RunCoordinateDescent(string functionStr, double x0, double y0,
            double epsilon, int maxIterations, double stepSize,
            Panel resultPanel, ref List<PointF> dataPoints)
        {
            resultPanel.Controls.Clear();

            try
            {
                double x = x0;
                double y = y0;
                double prevValue = EvaluateTwoVariableFunction(functionStr, x, y);
                int iteration = 0;
                bool converged = false;

                List<string> results = new List<string>();
                results.Add("=== ПОКООРДИНАТНЫЙ СПУСК ===");
                results.Add($"Функция: f(x,y) = {functionStr}");
                results.Add($"Начальная точка: ({x0:F4}, {y0:F4})");
                results.Add($"Шаг: {stepSize}, Точность: {epsilon}");
                results.Add("");

                // Основной цикл
                while (iteration < maxIterations && !converged)
                {
                    iteration++;

                    // Сохраняем предыдущие значения
                    double prevX = x;
                    double prevY = y;

                    // 1. Оптимизация по x (фиксируем y)
                    double fx1 = EvaluateTwoVariableFunction(functionStr, x + stepSize, y);
                    double fx2 = EvaluateTwoVariableFunction(functionStr, x - stepSize, y);
                    double fx = EvaluateTwoVariableFunction(functionStr, x, y);

                    if (fx1 < fx)
                    {
                        x += stepSize;
                    }
                    else if (fx2 < fx)
                    {
                        x -= stepSize;
                    }

                    // 2. Оптимизация по y (фиксируем обновленный x)
                    double fy1 = EvaluateTwoVariableFunction(functionStr, x, y + stepSize);
                    double fy2 = EvaluateTwoVariableFunction(functionStr, x, y - stepSize);
                    double fy = EvaluateTwoVariableFunction(functionStr, x, y);

                    if (fy1 < fy)
                    {
                        y += stepSize;
                    }
                    else if (fy2 < fy)
                    {
                        y -= stepSize;
                    }

                    // Вычисляем новое значение функции
                    double newValue = EvaluateTwoVariableFunction(functionStr, x, y);

                    // Сохраняем точку
                    dataPoints.Add(new PointF((float)x, (float)y));

                    // Проверка сходимости
                    double delta = Math.Abs(newValue - prevValue);
                    double distance = Math.Sqrt(Math.Pow(x - prevX, 2) + Math.Pow(y - prevY, 2));

                    if (delta < epsilon && distance < epsilon)
                    {
                        converged = true;
                    }

                    prevValue = newValue;

                    // Выводим информацию о итерации
                    if (iteration <= 10 || iteration % 10 == 0 || converged)
                    {
                        results.Add($"Итер. {iteration}: x={x:F6}, y={y:F6}, f={newValue:F6}");
                    }
                }

                // Формируем итоговый результат
                double finalValue = EvaluateTwoVariableFunction(functionStr, x, y);

                results.Add("");
                results.Add("=== РЕЗУЛЬТАТ ===");
                results.Add($"Минимум найден в точке:");
                results.Add($"x = {x:F8}");
                results.Add($"y = {y:F8}");
                results.Add($"f(x,y) = {finalValue:F8}");
                results.Add($"Итераций: {iteration}");
                results.Add(converged ? "Сошлось!" : "Достигнут лимит итераций");

                // Отображаем результаты
                DisplayDescentResults(resultPanel, results);
            }
            catch (Exception ex)
            {
                AddResultLabel(resultPanel, $"Ошибка: {ex.Message}", Color.Red);
            }
        }

        // Метод градиентного спуска
        private void RunGradientDescent(string functionStr, double x0, double y0,
            double epsilon, int maxIterations, double stepSize,
            Panel resultPanel, ref List<PointF> dataPoints)
        {
            resultPanel.Controls.Clear();

            try
            {
                double x = x0;
                double y = y0;
                double prevValue = EvaluateTwoVariableFunction(functionStr, x, y);
                int iteration = 0;
                bool converged = false;

                List<string> results = new List<string>();
                results.Add("=== ГРАДИЕНТНЫЙ СПУСК ===");
                results.Add($"Функция: f(x,y) = {functionStr}");
                results.Add($"Начальная точка: ({x0:F4}, {y0:F4})");
                results.Add($"Шаг: {stepSize}, Точность: {epsilon}");
                results.Add("");

                // Основной цикл
                while (iteration < maxIterations && !converged)
                {
                    iteration++;

                    // Вычисляем градиент (численно)
                    double h = 0.0001;

                    // Частная производная по x
                    double df_dx = (EvaluateTwoVariableFunction(functionStr, x + h, y) -
                                  EvaluateTwoVariableFunction(functionStr, x - h, y)) / (2 * h);

                    // Частная производная по y
                    double df_dy = (EvaluateTwoVariableFunction(functionStr, x, y + h) -
                                  EvaluateTwoVariableFunction(functionStr, x, y - h)) / (2 * h);

                    // Обновляем координаты
                    double newX = x - stepSize * df_dx;
                    double newY = y - stepSize * df_dy;

                    // Сохраняем точку
                    dataPoints.Add(new PointF((float)newX, (float)newY));

                    // Вычисляем новое значение функции
                    double newValue = EvaluateTwoVariableFunction(functionStr, newX, newY);

                    // Проверка сходимости
                    double delta = Math.Abs(newValue - prevValue);

                    if (delta < epsilon)
                    {
                        converged = true;
                    }

                    // Обновляем переменные
                    x = newX;
                    y = newY;
                    prevValue = newValue;

                    // Выводим информацию о итерации
                    if (iteration <= 10 || iteration % 10 == 0 || converged)
                    {
                        results.Add($"Итер. {iteration}: x={x:F6}, y={y:F6}, f={newValue:F6}");
                        results.Add($"  Градиент: ({df_dx:F6}, {df_dy:F6})");
                    }
                }

                // Формируем итоговый результат
                double finalValue = EvaluateTwoVariableFunction(functionStr, x, y);

                results.Add("");
                results.Add("=== РЕЗУЛЬТАТ ===");
                results.Add($"Минимум найден в точке:");
                results.Add($"x = {x:F8}");
                results.Add($"y = {y:F8}");
                results.Add($"f(x,y) = {finalValue:F8}");
                results.Add($"Итераций: {iteration}");
                results.Add(converged ? "Сошлось!" : "Достигнут лимит итераций");

                // Отображаем результаты
                DisplayDescentResults(resultPanel, results);
            }
            catch (Exception ex)
            {
                AddResultLabel(resultPanel, $"Ошибка: {ex.Message}", Color.Red);
            }
        }

        // Вычисление функции двух переменных
        private double EvaluateTwoVariableFunction(string expression, double x, double y)
        {
            try
            {
                // Подготовка выражения
                expression = expression.ToLower().Replace(" ", "");

                // Создаем парсер
                var parser = new MathParser();
                parser.LocalVariables["x"] = x;
                parser.LocalVariables["y"] = y;
                parser.LocalVariables["pi"] = Math.PI;
                parser.LocalVariables["e"] = Math.E;

                // Предварительная обработка
                expression = PreprocessTwoVariableExpression(expression);

                return parser.Parse(expression);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Ошибка вычисления функции: {ex.Message}");
            }
        }

        // Обработка выражения с двумя переменными
        private string PreprocessTwoVariableExpression(string expression)
        {
            // Уже есть метод PreprocessExpression, но он для одной переменной
            // Используем его как основу
            expression = PreprocessExpression(expression);

            // Дополнительные замены для двух переменных
            var replacements = new Dictionary<string, string>
    {
        { "sin(x)", "sin(x)" },
        { "cos(x)", "cos(x)" },
        { "exp(x)", "exp(x)" },
        { "sin(y)", "sin(y)" },
        { "cos(y)", "cos(y)" },
        { "exp(y)", "exp(y)" },
        { "x^2", "x^2" },
        { "y^2", "y^2" },
        { "x*y", "x*y" },
        { "y*x", "x*y" }
    };

            foreach (var replacement in replacements)
            {
                expression = expression.Replace(replacement.Key, replacement.Value);
            }

            return expression;
        }

        // Отображение результатов спуска
        private void DisplayDescentResults(Panel panel, List<string> results)
        {
            panel.Controls.Clear();

            int y = 10;
            foreach (string line in results)
            {
                var label = new Label
                {
                    Text = line,
                    Location = new Point(10, y),
                    AutoSize = true,
                    Font = new Font("Consolas", 9),
                    ForeColor = Color.Black
                };

                panel.Controls.Add(label);
                y += 20;
            }
        }

        // Отрисовка графика для метода спуска
        private void DrawDescentGraph(Graphics g, Rectangle drawingArea,
            string functionStr, List<PointF> dataPoints)
        {
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            if (string.IsNullOrEmpty(functionStr) || dataPoints.Count == 0)
            {
                g.DrawString("Введите функцию и найдите минимум",
                    new Font("Arial", 12), Brushes.Gray,
                    drawingArea.Width / 2 - 150, drawingArea.Height / 2 - 10);
                return;
            }

            // Определяем область графика
            int padding = 40;
            Rectangle graphArea = new Rectangle(
                drawingArea.Left + padding,
                drawingArea.Top + padding,
                drawingArea.Width - 2 * padding,
                drawingArea.Height - 2 * padding
            );

            // Находим диапазон координат
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var point in dataPoints)
            {
                minX = Math.Min(minX, point.X);
                maxX = Math.Max(maxX, point.X);
                minY = Math.Min(minY, point.Y);
                maxY = Math.Max(maxY, point.Y);
            }

            // Добавляем немного места по краям
            float rangeX = maxX - minX;
            float rangeY = maxY - minY;

            if (rangeX < 1) rangeX = 2;
            if (rangeY < 1) rangeY = 2;

            minX -= rangeX * 0.1f;
            maxX += rangeX * 0.1f;
            minY -= rangeY * 0.1f;
            maxY += rangeY * 0.1f;

            // Масштаб
            float scaleX = graphArea.Width / (maxX - minX);
            float scaleY = graphArea.Height / (maxY - minY);

            // Преобразуем точки в экранные координаты
            List<PointF> screenPoints = new List<PointF>();
            foreach (var point in dataPoints)
            {
                float screenX = graphArea.Left + (point.X - minX) * scaleX;
                float screenY = graphArea.Bottom - (point.Y - minY) * scaleY;
                screenPoints.Add(new PointF(screenX, screenY));
            }

            // Рисуем сетку
            DrawDescentGrid(g, graphArea, minX, maxX, minY, maxY, scaleX, scaleY);

            // Рисуем оси
            DrawDescentAxes(g, graphArea, minX, maxX, minY, maxY, scaleX, scaleY);

            // Рисуем линии уровня (изолинии)
            DrawContourLines(g, graphArea, functionStr, minX, maxX, minY, maxY, scaleX, scaleY);

            // Рисуем траекторию спуска
            if (screenPoints.Count >= 2)
            {
                using (Pen pathPen = new Pen(Color.Red, 2))
                {
                    for (int i = 0; i < screenPoints.Count - 1; i++)
                    {
                        g.DrawLine(pathPen, screenPoints[i], screenPoints[i + 1]);
                    }
                }
            }

            // Рисуем точки
            for (int i = 0; i < screenPoints.Count; i++)
            {
                var point = screenPoints[i];

                // Разные цвета для разных точек
                Brush pointBrush;
                if (i == 0)
                    pointBrush = Brushes.Green; // Начальная точка
                else if (i == screenPoints.Count - 1)
                    pointBrush = Brushes.Blue; // Конечная точка
                else
                    pointBrush = Brushes.Orange; // Промежуточные точки

                g.FillEllipse(pointBrush, point.X - 4, point.Y - 4, 8, 8);
                g.DrawEllipse(Pens.Black, point.X - 4, point.Y - 4, 8, 8);

                // Подписываем начальную и конечную точки
                if (i == 0)
                {
                    g.DrawString($"Начало: ({dataPoints[i].X:F2}, {dataPoints[i].Y:F2})",
                        new Font("Arial", 8), Brushes.DarkGreen,
                        point.X + 5, point.Y - 10);
                }
                else if (i == screenPoints.Count - 1)
                {
                    g.DrawString($"Конец: ({dataPoints[i].X:F4}, {dataPoints[i].Y:F4})",
                        new Font("Arial", 8), Brushes.DarkBlue,
                        point.X + 5, point.Y + 5);
                }
            }

            // Подписи
            DrawDescentLabels(g, graphArea, drawingArea, functionStr, dataPoints);
        }

        // Рисуем сетку для графика спуска
        private void DrawDescentGrid(Graphics g, Rectangle graphArea,
            float minX, float maxX, float minY, float maxY,
            float scaleX, float scaleY)
        {
            Pen gridPen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dot };
            Font gridFont = new Font("Arial", 8);

            // Вертикальные линии
            int xDivisions = 10;
            for (int i = 0; i <= xDivisions; i++)
            {
                float xValue = minX + (maxX - minX) * i / xDivisions;
                float screenX = graphArea.Left + (xValue - minX) * scaleX;

                g.DrawLine(gridPen, screenX, graphArea.Top, screenX, graphArea.Bottom);

                // Подпись
                string label = xValue.ToString("F1");
                SizeF textSize = g.MeasureString(label, gridFont);
                g.DrawString(label, gridFont, Brushes.Gray,
                    screenX - textSize.Width / 2, graphArea.Bottom + 5);
            }

            // Горизонтальные линии
            int yDivisions = 10;
            for (int i = 0; i <= yDivisions; i++)
            {
                float yValue = minY + (maxY - minY) * i / yDivisions;
                float screenY = graphArea.Bottom - (yValue - minY) * scaleY;

                g.DrawLine(gridPen, graphArea.Left, screenY, graphArea.Right, screenY);

                // Подпись
                string label = yValue.ToString("F1");
                SizeF textSize = g.MeasureString(label, gridFont);
                g.DrawString(label, gridFont, Brushes.Gray,
                    graphArea.Left - textSize.Width - 5, screenY - textSize.Height / 2);
            }
        }

        // Рисуем оси для графика спуска
        private void DrawDescentAxes(Graphics g, Rectangle graphArea,
            float minX, float maxX, float minY, float maxY,
            float scaleX, float scaleY)
        {
            Pen axisPen = new Pen(Color.Black, 2);
            Font axisFont = new Font("Arial", 9, FontStyle.Bold);

            // Ось X (если 0 в диапазоне Y)
            if (minY <= 0 && maxY >= 0)
            {
                float zeroY = graphArea.Bottom - (0 - minY) * scaleY;
                g.DrawLine(axisPen, graphArea.Left, zeroY, graphArea.Right, zeroY);

                // Стрелка
                g.DrawLine(axisPen, graphArea.Right - 10, zeroY - 5, graphArea.Right, zeroY);
                g.DrawLine(axisPen, graphArea.Right - 10, zeroY + 5, graphArea.Right, zeroY);

                // Подпись
                g.DrawString("X", axisFont, Brushes.Black, graphArea.Right - 15, zeroY - 20);
            }

            // Ось Y (если 0 в диапазоне X)
            if (minX <= 0 && maxX >= 0)
            {
                float zeroX = graphArea.Left + (0 - minX) * scaleX;
                g.DrawLine(axisPen, zeroX, graphArea.Top, zeroX, graphArea.Bottom);

                // Стрелка
                g.DrawLine(axisPen, zeroX - 5, graphArea.Top + 10, zeroX, graphArea.Top);
                g.DrawLine(axisPen, zeroX + 5, graphArea.Top + 10, zeroX, graphArea.Top);

                // Подпись
                g.DrawString("Y", axisFont, Brushes.Black, zeroX + 10, graphArea.Top);
            }
        }

        // Рисуем линии уровня (изолинии)
        private void DrawContourLines(Graphics g, Rectangle graphArea, string functionStr,
            float minX, float maxX, float minY, float maxY,
            float scaleX, float scaleY)
        {
            if (string.IsNullOrEmpty(functionStr))
                return;

            try
            {
                Pen contourPen = new Pen(Color.FromArgb(100, Color.Blue), 1);

                // Количество линий уровня
                int contourCount = 10;

                // Находим минимальное и максимальное значение функции в области
                double minF = double.MaxValue;
                double maxF = double.MinValue;
                int samples = 20;

                for (int i = 0; i <= samples; i++)
                {
                    for (int j = 0; j <= samples; j++)
                    {
                        double x = minX + (maxX - minX) * i / samples;
                        double y = minY + (maxY - minY) * j / samples;

                        try
                        {
                            double f = EvaluateTwoVariableFunction(functionStr, x, y);
                            minF = Math.Min(minF, f);
                            maxF = Math.Max(maxF, f);
                        }
                        catch { }
                    }
                }

                // Рисуем линии уровня
                for (int level = 0; level <= contourCount; level++)
                {
                    double fValue = minF + (maxF - minF) * level / contourCount;

                    List<PointF> contourPoints = new List<PointF>();

                    // Проходим по области и ищем точки, где функция равна fValue
                    int gridSize = 50;
                    for (int i = 0; i < gridSize; i++)
                    {
                        for (int j = 0; j < gridSize; j++)
                        {
                            double x1 = minX + (maxX - minX) * i / gridSize;
                            double x2 = minX + (maxX - minX) * (i + 1) / gridSize;
                            double y1 = minY + (maxY - minY) * j / gridSize;
                            double y2 = minY + (maxY - minY) * (j + 1) / gridSize;

                            // Проверяем четыре угла квадрата
                            double[] values = new double[4];
                            try { values[0] = EvaluateTwoVariableFunction(functionStr, x1, y1); } catch { values[0] = double.NaN; }
                            try { values[1] = EvaluateTwoVariableFunction(functionStr, x2, y1); } catch { values[1] = double.NaN; }
                            try { values[2] = EvaluateTwoVariableFunction(functionStr, x1, y2); } catch { values[2] = double.NaN; }
                            try { values[3] = EvaluateTwoVariableFunction(functionStr, x2, y2); } catch { values[3] = double.NaN; }

                            // Ищем пересечения с изолинией
                            for (int edge = 0; edge < 4; edge++)
                            {
                                // Пропускаем NaN
                                if (double.IsNaN(values[edge]) || double.IsNaN(values[(edge + 1) % 4]))
                                    continue;

                                // Проверяем, проходит ли изолиния через это ребро
                                if ((values[edge] <= fValue && values[(edge + 1) % 4] >= fValue) ||
                                    (values[edge] >= fValue && values[(edge + 1) % 4] <= fValue))
                                {
                                    // Линейная интерполяция
                                    double t = (fValue - values[edge]) / (values[(edge + 1) % 4] - values[edge]);
                                    double interpX, interpY;

                                    switch (edge)
                                    {
                                        case 0: // от (x1,y1) к (x2,y1)
                                            interpX = x1 + t * (x2 - x1);
                                            interpY = y1;
                                            break;
                                        case 1: // от (x2,y1) к (x2,y2)
                                            interpX = x2;
                                            interpY = y1 + t * (y2 - y1);
                                            break;
                                        case 2: // от (x1,y2) к (x2,y2)
                                            interpX = x1 + t * (x2 - x1);
                                            interpY = y2;
                                            break;
                                        case 3: // от (x1,y1) к (x1,y2)
                                            interpX = x1;
                                            interpY = y1 + t * (y2 - y1);
                                            break;
                                        default:
                                            continue;
                                    }

                                    // Преобразуем в экранные координаты
                                    float screenX = graphArea.Left + (float)((interpX - minX) * scaleX);
                                    float screenY = graphArea.Bottom - (float)((interpY - minY) * scaleY);

                                    contourPoints.Add(new PointF(screenX, screenY));
                                }
                            }
                        }
                    }

                    // Рисуем линию уровня
                    if (contourPoints.Count >= 2)
                    {
                        // Сортируем точки для создания непрерывной линии
                        contourPoints = contourPoints.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

                        // Рисуем отдельные сегменты
                        for (int i = 0; i < contourPoints.Count - 1; i++)
                        {
                            float dx = Math.Abs(contourPoints[i + 1].X - contourPoints[i].X);
                            float dy = Math.Abs(contourPoints[i + 1].Y - contourPoints[i].Y);

                            // Пропускаем слишком длинные сегменты (разрывы)
                            if (dx < graphArea.Width / 10 && dy < graphArea.Height / 10)
                            {
                                g.DrawLine(contourPen, contourPoints[i], contourPoints[i + 1]);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки при отрисовке линий уровня
            }
        }

        // Подписи для графика спуска
        private void DrawDescentLabels(Graphics g, Rectangle graphArea, Rectangle drawingArea,
            string functionStr, List<PointF> dataPoints)
        {
            Font titleFont = new Font("Arial", 11, FontStyle.Bold);
            Font infoFont = new Font("Arial", 9);

            // Заголовок
            string title = $"Метод спуска: f(x,y) = {functionStr}";
            g.DrawString(title, titleFont, Brushes.DarkBlue,
                drawingArea.Left + 10, drawingArea.Top + 5);

            // Информация о траектории
            if (dataPoints.Count > 1)
            {
                string startInfo = $"Начало: ({dataPoints[0].X:F2}, {dataPoints[0].Y:F2})";
                string endInfo = $"Конец: ({dataPoints.Last().X:F4}, {dataPoints.Last().Y:F4})";
                string stepsInfo = $"Шагов: {dataPoints.Count - 1}";

                g.DrawString(startInfo, infoFont, Brushes.DarkGreen,
                    drawingArea.Left + 10, drawingArea.Top + 30);
                g.DrawString(endInfo, infoFont, Brushes.DarkBlue,
                    drawingArea.Left + 10, drawingArea.Top + 50);
                g.DrawString(stepsInfo, infoFont, Brushes.DarkRed,
                    drawingArea.Left + 10, drawingArea.Top + 70);
            }

            // Легенда
            string legend = "Легенда: ● - начальная точка, ● - траектория, ● - конечная точка";
            g.DrawString(legend, new Font("Arial", 8), Brushes.DarkGray,
                graphArea.Left, graphArea.Bottom + 5);
        }

        //***************************************************************************************| МЕТОД НАИМЕНЬШИХ КВАДРАТОВ |*****************************************************************************//

        // ИНТЕРФЕЙС
        private void InitializeLeastSquares()
        {
            // Очищаем панель
            panel1.Controls.Clear();

            // Данные для аппроксимации
            List<PointF> dataPoints = new List<PointF>();
            List<double> coefficients = new List<double>();
            string currentLSFunction = "";

            // Группа для ввода данных
            var groupBoxData = new GroupBox
            {
                Text = "Ввод данных (точки x,y)",
                Location = new Point(10, 10),
                Size = new Size(310, 120),
                BackColor = Color.Lavender
            };

            var labelX = new Label { Text = "X:", Location = new Point(10, 25), AutoSize = true };
            var textBoxX = new System.Windows.Forms.TextBox { Location = new Point(30, 25), Width = 70, BackColor = Color.WhiteSmoke };

            var labelY = new Label { Text = "Y:", Location = new Point(110, 25), AutoSize = true };
            var textBoxY = new System.Windows.Forms.TextBox { Location = new Point(130, 25), Width = 70, BackColor = Color.WhiteSmoke };

            var btnAddPoint = new System.Windows.Forms.Button
            {
                Text = "Добавить точку",
                Location = new Point(210, 23),
                BackColor = Color.LightBlue,
                Width = 90,
                Height = 25
            };

            var btnClearPoints = new System.Windows.Forms.Button
            {
                Text = "Очистить все",
                Location = new Point(210, 53),
                BackColor = Color.LightCoral,
                Width = 90,
                Height = 25
            };

            var btnGenerateRandom = new System.Windows.Forms.Button
            {
                Text = "Случайные",
                Location = new Point(210, 83),
                BackColor = Color.LightGreen,
                Width = 90,
                Height = 25
            };

            // Список точек
            var listBoxPoints = new ListBox
            {
                Location = new Point(10, 55),
                Size = new Size(190, 60),
                BackColor = Color.WhiteSmoke
            };

            groupBoxData.Controls.AddRange(new Control[] { labelX, textBoxX, labelY, textBoxY, btnAddPoint,
        btnClearPoints, btnGenerateRandom, listBoxPoints });

            // Группа для выбора типа аппроксимации
            var groupBoxApprox = new GroupBox
            {
                Text = "Тип аппроксимации",
                Location = new Point(10, 140),
                Size = new Size(310, 120),
                BackColor = Color.Lavender
            };

            var rbLinear = new RadioButton { Text = "Линейная (y = a + b*x)", Location = new Point(10, 20), Width = 200, Checked = true };
            var rbQuadratic = new RadioButton { Text = "Квадратичная (y = a + b*x + c*x²)", Location = new Point(10, 45), Width = 250 };
            var rbCubic = new RadioButton { Text = "Кубическая (y = a + b*x + c*x² + d*x³)", Location = new Point(10, 70), Width = 280 };
            var rbExponential = new RadioButton { Text = "Экспоненциальная (y = a*e^(b*x))", Location = new Point(10, 95), Width = 250 };

            groupBoxApprox.Controls.AddRange(new Control[] { rbLinear, rbQuadratic, rbCubic, rbExponential });

            // Кнопки расчета
            var btnCalculate = new System.Windows.Forms.Button
            {
                Text = "Рассчитать",
                Location = new Point(10, 270),
                BackColor = Color.MediumSeaGreen,
                ForeColor = Color.White,
                Width = 150,
                Height = 30
            };

            var btnExample1 = new System.Windows.Forms.Button
            {
                Text = "Пример 1: Линейная",
                Location = new Point(170, 270),
                BackColor = Color.LightBlue,
                Width = 150,
                Height = 30
            };

            var btnExample2 = new System.Windows.Forms.Button
            {
                Text = "Пример 2: Квадратичная",
                Location = new Point(10, 300),
                BackColor = Color.LightBlue,
                Width = 150,
                Height = 30
            };

            var btnExample3 = new System.Windows.Forms.Button
            {
                Text = "Пример 3: Экспонента",
                Location = new Point(170, 300),
                BackColor = Color.LightBlue,
                Width = 150,
                Height = 30
            };

            // Панель для результатов
            var resultPanel = new Panel
            {
                Location = new Point(10, 340),
                Size = new Size(310, 130),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                BackColor = Color.WhiteSmoke
            };

            // Панель для графика
            var graphPanel = new Panel
            {
                Location = new Point(330, 10),
                Size = new Size(440, 460),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            // Обработчики событий
            btnAddPoint.Click += (s, e) =>
            {
                try
                {
                    float x = float.Parse(textBoxX.Text);
                    float y = float.Parse(textBoxY.Text);

                    dataPoints.Add(new PointF(x, y));
                    listBoxPoints.Items.Add($"({x:F2}, {y:F2})");

                    textBoxX.Text = "";
                    textBoxY.Text = "";
                    textBoxX.Focus();
                }
                catch
                {
                    MessageBox.Show("Введите корректные числа!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnClearPoints.Click += (s, e) =>
            {
                dataPoints.Clear();
                listBoxPoints.Items.Clear();
                coefficients.Clear();
                graphPanel.Invalidate();
                resultPanel.Controls.Clear();
            };

            btnGenerateRandom.Click += (s, e) =>
            {
                dataPoints.Clear();
                listBoxPoints.Items.Clear();

                Random rand = new Random();
                int count = rand.Next(5, 15);

                for (int i = 0; i < count; i++)
                {
                    float x = i * 2 + rand.Next(-5, 5) * 0.5f;
                    float y = 2 * x + 3 + rand.Next(-10, 10) * 0.5f;
                    dataPoints.Add(new PointF(x, y));
                    listBoxPoints.Items.Add($"({x:F2}, {y:F2})");
                }
            };

            // Примеры данных
            btnExample1.Click += (s, e) =>
            {
                dataPoints.Clear();
                listBoxPoints.Items.Clear();

                // Линейная зависимость с шумом
                float[] xVals = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
                float[] yVals = { 2.1f, 4.3f, 5.8f, 8.2f, 9.9f, 11.5f, 13.8f, 15.1f, 17.4f, 19.2f };

                for (int i = 0; i < xVals.Length; i++)
                {
                    dataPoints.Add(new PointF(xVals[i], yVals[i]));
                    listBoxPoints.Items.Add($"({xVals[i]:F2}, {yVals[i]:F2})");
                }

                rbLinear.Checked = true;
            };

            btnExample2.Click += (s, e) =>
            {
                dataPoints.Clear();
                listBoxPoints.Items.Clear();

                // Квадратичная зависимость
                float[] xVals = { -3, -2, -1, 0, 1, 2, 3, 4 };
                float[] yVals = { 10.5f, 3.8f, 0.9f, 1.2f, 2.5f, 6.8f, 12.3f, 20.6f };

                for (int i = 0; i < xVals.Length; i++)
                {
                    dataPoints.Add(new PointF(xVals[i], yVals[i]));
                    listBoxPoints.Items.Add($"({xVals[i]:F2}, {yVals[i]:F2})");
                }

                rbQuadratic.Checked = true;
            };

            btnExample3.Click += (s, e) =>
            {
                dataPoints.Clear();
                listBoxPoints.Items.Clear();

                // Экспоненциальная зависимость
                float[] xVals = { 0, 1, 2, 3, 4, 5 };
                float[] yVals = { 1.2f, 3.3f, 9.1f, 25.4f, 68.3f, 184.7f };

                for (int i = 0; i < xVals.Length; i++)
                {
                    dataPoints.Add(new PointF(xVals[i], yVals[i]));
                    listBoxPoints.Items.Add($"({xVals[i]:F2}, {yVals[i]:F2})");
                }

                rbExponential.Checked = true;
            };

            // Основной обработчик расчета
            btnCalculate.Click += (s, e) =>
            {
                if (dataPoints.Count < 2)
                {
                    MessageBox.Show("Добавьте хотя бы 2 точки!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    coefficients.Clear();

                    if (rbLinear.Checked)
                    {
                        coefficients = CalculateLinearRegression(dataPoints);
                        currentLSFunction = $"y = {coefficients[0]:F4} + {coefficients[1]:F4}*x";
                    }
                    else if (rbQuadratic.Checked)
                    {
                        coefficients = CalculateQuadraticRegression(dataPoints);
                        currentLSFunction = $"y = {coefficients[0]:F4} + {coefficients[1]:F4}*x + {coefficients[2]:F4}*x²";
                    }
                    else if (rbCubic.Checked)
                    {
                        coefficients = CalculateCubicRegression(dataPoints);
                        currentLSFunction = $"y = {coefficients[0]:F4} + {coefficients[1]:F4}*x + {coefficients[2]:F4}*x² + {coefficients[3]:F4}*x³";
                    }
                    else if (rbExponential.Checked)
                    {
                        coefficients = CalculateExponentialRegression(dataPoints);
                        currentLSFunction = $"y = {coefficients[0]:F4} * e^({coefficients[1]:F4}*x)";
                    }

                    DisplayLSResults(resultPanel, dataPoints, coefficients, currentLSFunction);
                    graphPanel.Invalidate();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка расчета: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Обработчик отрисовки графика
            graphPanel.Paint += (sender, e) =>
            {
                DrawLSGraph(e.Graphics, graphPanel.ClientRectangle,
                    dataPoints, coefficients, currentLSFunction);
            };

            // Добавляем элементы на панель
            panel1.Controls.AddRange(new Control[]
            {
        groupBoxData,
        groupBoxApprox,
        btnCalculate, btnExample1, btnExample2, btnExample3,
        resultPanel,
        graphPanel
            });
        }

        // Линейная регрессия (y = a + b*x)
        private List<double> CalculateLinearRegression(List<PointF> points)
        {
            int n = points.Count;

            // Суммы
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

            foreach (var point in points)
            {
                double x = point.X;
                double y = point.Y;

                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }

            // Коэффициенты
            double b = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double a = (sumY - b * sumX) / n;

            return new List<double> { a, b };
        }

        // Квадратичная регрессия (y = a + b*x + c*x²)
        private List<double> CalculateQuadraticRegression(List<PointF> points)
        {
            int n = points.Count;

            // Суммы
            double sumX = 0, sumY = 0, sumX2 = 0, sumX3 = 0, sumX4 = 0, sumXY = 0, sumX2Y = 0;

            foreach (var point in points)
            {
                double x = point.X;
                double y = point.Y;
                double x2 = x * x;
                double x3 = x2 * x;
                double x4 = x3 * x;

                sumX += x;
                sumY += y;
                sumX2 += x2;
                sumX3 += x3;
                sumX4 += x4;
                sumXY += x * y;
                sumX2Y += x2 * y;
            }

            // Матрица системы уравнений
            double[,] matrix = new double[3, 3];
            double[] vector = new double[3];

            matrix[0, 0] = n;
            matrix[0, 1] = sumX;
            matrix[0, 2] = sumX2;
            matrix[1, 0] = sumX;
            matrix[1, 1] = sumX2;
            matrix[1, 2] = sumX3;
            matrix[2, 0] = sumX2;
            matrix[2, 1] = sumX3;
            matrix[2, 2] = sumX4;

            vector[0] = sumY;
            vector[1] = sumXY;
            vector[2] = sumX2Y;

            // Решение системы методом Гаусса
            return SolveLinearSystem(matrix, vector);
        }

        // Кубическая регрессия (y = a + b*x + c*x² + d*x³)
        private List<double> CalculateCubicRegression(List<PointF> points)
        {
            int n = points.Count;

            // Суммы
            double sumX = 0, sumY = 0, sumX2 = 0, sumX3 = 0, sumX4 = 0, sumX5 = 0, sumX6 = 0;
            double sumXY = 0, sumX2Y = 0, sumX3Y = 0;

            foreach (var point in points)
            {
                double x = point.X;
                double y = point.Y;
                double x2 = x * x;
                double x3 = x2 * x;
                double x4 = x3 * x;
                double x5 = x4 * x;
                double x6 = x5 * x;

                sumX += x;
                sumY += y;
                sumX2 += x2;
                sumX3 += x3;
                sumX4 += x4;
                sumX5 += x5;
                sumX6 += x6;
                sumXY += x * y;
                sumX2Y += x2 * y;
                sumX3Y += x3 * y;
            }

            // Матрица системы уравнений
            double[,] matrix = new double[4, 4];
            double[] vector = new double[4];

            matrix[0, 0] = n;
            matrix[0, 1] = sumX;
            matrix[0, 2] = sumX2;
            matrix[0, 3] = sumX3;

            matrix[1, 0] = sumX;
            matrix[1, 1] = sumX2;
            matrix[1, 2] = sumX3;
            matrix[1, 3] = sumX4;

            matrix[2, 0] = sumX2;
            matrix[2, 1] = sumX3;
            matrix[2, 2] = sumX4;
            matrix[2, 3] = sumX5;

            matrix[3, 0] = sumX3;
            matrix[3, 1] = sumX4;
            matrix[3, 2] = sumX5;
            matrix[3, 3] = sumX6;

            vector[0] = sumY;
            vector[1] = sumXY;
            vector[2] = sumX2Y;
            vector[3] = sumX3Y;

            // Решение системы методом Гаусса
            return SolveLinearSystem(matrix, vector);
        }

        // Экспоненциальная регрессия (y = a * e^(b*x))
        private List<double> CalculateExponentialRegression(List<PointF> points)
        {
            // Линеаризация: ln(y) = ln(a) + b*x
            List<PointF> linearPoints = new List<PointF>();

            foreach (var point in points)
            {
                if (point.Y <= 0)
                {
                    throw new ArgumentException("Для экспоненциальной регрессии все y должны быть > 0");
                }

                linearPoints.Add(new PointF(point.X, (float)Math.Log(point.Y)));
            }

            // Линейная регрессия для ln(y)
            var coefficients = CalculateLinearRegression(linearPoints);

            // Преобразование коэффициентов
            double lnA = coefficients[0];
            double b = coefficients[1];
            double a = Math.Exp(lnA);

            return new List<double> { a, b };
        }

        // Решение системы линейных уравнений методом Гаусса
        private List<double> SolveLinearSystem(double[,] matrix, double[] vector)
        {
            int n = vector.Length;

            // Прямой ход метода Гаусса
            for (int i = 0; i < n; i++)
            {
                // Поиск максимального элемента в столбце
                int maxRow = i;
                double maxVal = Math.Abs(matrix[i, i]);

                for (int j = i + 1; j < n; j++)
                {
                    if (Math.Abs(matrix[j, i]) > maxVal)
                    {
                        maxVal = Math.Abs(matrix[j, i]);
                        maxRow = j;
                    }
                }

                // Перестановка строк
                if (maxRow != i)
                {
                    for (int k = i; k < n; k++)
                    {
                        double temp = matrix[i, k];
                        matrix[i, k] = matrix[maxRow, k];
                        matrix[maxRow, k] = temp;
                    }

                    double tempVec = vector[i];
                    vector[i] = vector[maxRow];
                    vector[maxRow] = tempVec;
                }

                // Приведение к треугольному виду
                for (int j = i + 1; j < n; j++)
                {
                    double factor = matrix[j, i] / matrix[i, i];

                    for (int k = i; k < n; k++)
                    {
                        matrix[j, k] -= factor * matrix[i, k];
                    }

                    vector[j] -= factor * vector[i];
                }
            }

            // Обратный ход
            List<double> solution = new List<double>(new double[n]);

            for (int i = n - 1; i >= 0; i--)
            {
                double sum = 0;

                for (int j = i + 1; j < n; j++)
                {
                    sum += matrix[i, j] * solution[j];
                }

                solution[i] = (vector[i] - sum) / matrix[i, i];
            }

            return solution;
        }

        // Отображение результатов МНК
        private void DisplayLSResults(Panel panel, List<PointF> points, List<double> coefficients, string function)
        {
            panel.Controls.Clear();

            List<string> results = new List<string>();
            results.Add("=== РЕЗУЛЬТАТЫ АППРОКСИМАЦИИ ===");
            results.Add($"Функция: {function}");
            results.Add("");

            // Вывод коэффициентов
            results.Add("Коэффициенты:");
            for (int i = 0; i < coefficients.Count; i++)
            {
                string coefName = i == 0 ? "a" : i == 1 ? "b" : i == 2 ? "c" : "d";
                results.Add($"  {coefName} = {coefficients[i]:F6}");
            }

            results.Add("");

            // Расчет ошибок
            double sumSquaredErrors = 0;
            double sumAbsoluteErrors = 0;
            double maxError = 0;

            results.Add("Ошибки для каждой точки:");
            for (int i = 0; i < points.Count; i++)
            {
                double xVal = points[i].X;           // Изменили имя на xVal
                double yVal = points[i].Y;           // Изменили имя на yVal
                double yPredicted = CalculatePredictedY(xVal, coefficients);
                double error = yPredicted - yVal;
                double absError = Math.Abs(error);

                sumSquaredErrors += error * error;
                sumAbsoluteErrors += absError;
                maxError = Math.Max(maxError, absError);

                if (i < 10) // Показываем только первые 10 точек
                {
                    results.Add($"  Точка {i + 1}: y={yVal:F2}, ŷ={yPredicted:F2}, ошибка={error:F4}");
                }
            }

            if (points.Count > 10)
            {
                results.Add($"  ... и еще {points.Count - 10} точек");
            }

            results.Add("");

            // Статистика
            double mse = sumSquaredErrors / points.Count; // Среднеквадратичная ошибка
            double rmse = Math.Sqrt(mse);                 // Корень из MSE
            double mae = sumAbsoluteErrors / points.Count; // Средняя абсолютная ошибка

            results.Add("СТАТИСТИКА:");
            results.Add($"  Кол-во точек: {points.Count}");
            results.Add($"  Сумма квадратов ошибок: {sumSquaredErrors:F6}");
            results.Add($"  Среднеквадратичная ошибка (MSE): {mse:F6}");
            results.Add($"  Корень из MSE (RMSE): {rmse:F6}");
            results.Add($"  Средняя абсолютная ошибка (MAE): {mae:F6}");
            results.Add($"  Максимальная ошибка: {maxError:F6}");

            // Коэффициент детерминации R²
            if (points.Count > 1)
            {
                double meanY = points.Average(p => p.Y);
                double totalSumSquares = points.Sum(p => Math.Pow(p.Y - meanY, 2));
                double rSquared = 1 - (sumSquaredErrors / totalSumSquares);

                results.Add($"  Коэффициент детерминации R²: {rSquared:F6}");
                results.Add($"  Качество аппроксимации: {(rSquared > 0.9 ? "Отличное" : rSquared > 0.7 ? "Хорошее" : rSquared > 0.5 ? "Удовлетворительное" : "Плохое")}");
            }

            // Отображаем результаты
            int currentY = 10;  // Изменили имя переменной на currentY
            foreach (string line in results)
            {
                var label = new Label
                {
                    Text = line,
                    Location = new Point(10, currentY),
                    AutoSize = true,
                    Font = new Font("Consolas", 8),
                    ForeColor = Color.Black
                };

                panel.Controls.Add(label);
                currentY += 18;  // Изменили здесь тоже
            }
        }

        // Вычисление предсказанного значения y для заданного x
        private double CalculatePredictedY(double x, List<double> coefficients)
        {
            if (coefficients == null || coefficients.Count == 0)
                return 0;

            double result = coefficients[0]; // Свободный член

            for (int i = 1; i < coefficients.Count; i++)
            {
                result += coefficients[i] * Math.Pow(x, i);
            }

            return result;
        }

        // Отрисовка графика для МНК
        private void DrawLSGraph(Graphics g, Rectangle drawingArea,
            List<PointF> points, List<double> coefficients, string function)
        {
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            if (points.Count == 0)
            {
                g.DrawString("Добавьте точки данных",
                    new Font("Arial", 12), Brushes.Gray,
                    drawingArea.Width / 2 - 100, drawingArea.Height / 2 - 10);
                return;
            }

            // Определяем область графика
            int padding = 50;
            Rectangle graphArea = new Rectangle(
                drawingArea.Left + padding,
                drawingArea.Top + padding,
                drawingArea.Width - 2 * padding,
                drawingArea.Height - 2 * padding
            );

            // Находим диапазон данных
            float minX = points.Min(p => p.X);
            float maxX = points.Max(p => p.X);
            float minY = points.Min(p => p.Y);
            float maxY = points.Max(p => p.Y);

            // Добавляем немного места по краям
            float rangeX = maxX - minX;
            float rangeY = maxY - minY;

            if (rangeX < 0.1f) rangeX = 1;
            if (rangeY < 0.1f) rangeY = 1;

            minX -= rangeX * 0.1f;
            maxX += rangeX * 0.1f;
            minY -= rangeY * 0.1f;
            maxY += rangeY * 0.1f;

            // Масштаб
            float scaleX = graphArea.Width / (maxX - minX);
            float scaleY = graphArea.Height / (maxY - minY);

            // Функция преобразования координат
            PointF TransformPoint(float x, float y)
            {
                return new PointF(
                    graphArea.Left + (x - minX) * scaleX,
                    graphArea.Bottom - (y - minY) * scaleY
                );
            }

            // Рисуем сетку
            DrawLSGrid(g, graphArea, minX, maxX, minY, maxY, scaleX, scaleY);

            // Рисуем оси
            DrawLSAxes(g, graphArea, minX, maxX, minY, maxY, scaleX, scaleY);

            // Рисуем аппроксимирующую кривую
            if (coefficients != null && coefficients.Count > 0)
            {
                using (Pen curvePen = new Pen(Color.Red, 2))
                {
                    List<PointF> curvePoints = new List<PointF>();

                    int segments = 200;
                    for (int i = 0; i <= segments; i++)
                    {
                        float x = minX + (maxX - minX) * i / segments;
                        double y = CalculatePredictedY(x, coefficients);

                        // Ограничиваем слишком большие значения
                        if (double.IsInfinity(y) || double.IsNaN(y) || Math.Abs(y) > Math.Abs(maxY) * 10)
                            continue;

                        PointF point = TransformPoint(x, (float)y);
                        curvePoints.Add(point);
                    }

                    // Рисуем сглаженную кривую
                    if (curvePoints.Count >= 2)
                    {
                        for (int i = 0; i < curvePoints.Count - 1; i++)
                        {
                            g.DrawLine(curvePen, curvePoints[i], curvePoints[i + 1]);
                        }
                    }
                }
            }

            // Рисуем точки данных
            foreach (var point in points)
            {
                PointF screenPoint = TransformPoint(point.X, point.Y);

                // Рисуем точку
                g.FillEllipse(Brushes.Blue, screenPoint.X - 4, screenPoint.Y - 4, 8, 8);
                g.DrawEllipse(Pens.DarkBlue, screenPoint.X - 4, screenPoint.Y - 4, 8, 8);

                // Рисуем вертикальную линию до кривой (ошибка)
                if (coefficients != null && coefficients.Count > 0)
                {
                    double yPredicted = CalculatePredictedY(point.X, coefficients);
                    PointF predictedPoint = TransformPoint(point.X, (float)yPredicted);

                    using (Pen errorPen = new Pen(Color.FromArgb(100, Color.Red), 1))
                    {
                        g.DrawLine(errorPen, screenPoint, predictedPoint);
                    }

                    // Точка на кривой
                    g.FillEllipse(Brushes.Red, predictedPoint.X - 3, predictedPoint.Y - 3, 6, 6);
                }
            }

            // Подписи
            DrawLSLabels(g, graphArea, drawingArea, points, function);
        }

        // Рисуем сетку для графика МНК
        private void DrawLSGrid(Graphics g, Rectangle graphArea,
            float minX, float maxX, float minY, float maxY,
            float scaleX, float scaleY)
        {
            Pen gridPen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dot };
            Font gridFont = new Font("Arial", 8);

            // Вертикальные линии
            int xDivisions = 10;
            for (int i = 0; i <= xDivisions; i++)
            {
                float xValue = minX + (maxX - minX) * i / xDivisions;
                float screenX = graphArea.Left + (xValue - minX) * scaleX;

                g.DrawLine(gridPen, screenX, graphArea.Top, screenX, graphArea.Bottom);

                // Подпись
                string label = xValue.ToString("F1");
                SizeF textSize = g.MeasureString(label, gridFont);
                g.DrawString(label, gridFont, Brushes.Gray,
                    screenX - textSize.Width / 2, graphArea.Bottom + 5);
            }

            // Горизонтальные линии
            int yDivisions = 10;
            for (int i = 0; i <= yDivisions; i++)
            {
                float yValue = minY + (maxY - minY) * i / yDivisions;
                float screenY = graphArea.Bottom - (yValue - minY) * scaleY;

                g.DrawLine(gridPen, graphArea.Left, screenY, graphArea.Right, screenY);

                // Подпись
                string label = yValue.ToString("F1");
                SizeF textSize = g.MeasureString(label, gridFont);
                g.DrawString(label, gridFont, Brushes.Gray,
                    graphArea.Left - textSize.Width - 5, screenY - textSize.Height / 2);
            }
        }

        // Рисуем оси для графика МНК
        private void DrawLSAxes(Graphics g, Rectangle graphArea,
            float minX, float maxX, float minY, float maxY,
            float scaleX, float scaleY)
        {
            Pen axisPen = new Pen(Color.Black, 2);
            Font axisFont = new Font("Arial", 9, FontStyle.Bold);

            // Ось X
            if (minY <= 0 && maxY >= 0)
            {
                float zeroY = graphArea.Bottom - (0 - minY) * scaleY;
                g.DrawLine(axisPen, graphArea.Left, zeroY, graphArea.Right, zeroY);

                // Стрелка
                g.DrawLine(axisPen, graphArea.Right - 10, zeroY - 5, graphArea.Right, zeroY);
                g.DrawLine(axisPen, graphArea.Right - 10, zeroY + 5, graphArea.Right, zeroY);

                // Подпись
                g.DrawString("X", axisFont, Brushes.Black, graphArea.Right - 15, zeroY - 20);
            }

            // Ось Y
            if (minX <= 0 && maxX >= 0)
            {
                float zeroX = graphArea.Left + (0 - minX) * scaleX;
                g.DrawLine(axisPen, zeroX, graphArea.Top, zeroX, graphArea.Bottom);

                // Стрелка
                g.DrawLine(axisPen, zeroX - 5, graphArea.Top + 10, zeroX, graphArea.Top);
                g.DrawLine(axisPen, zeroX + 5, graphArea.Top + 10, zeroX, graphArea.Top);

                // Подпись
                g.DrawString("Y", axisFont, Brushes.Black, zeroX + 10, graphArea.Top);
            }
        }

        // Подписи для графика МНК
        private void DrawLSLabels(Graphics g, Rectangle graphArea, Rectangle drawingArea,
            List<PointF> points, string function)
        {
            Font titleFont = new Font("Arial", 11, FontStyle.Bold);
            Font infoFont = new Font("Arial", 9);

            // Заголовок
            string title = "МЕТОД НАИМЕНЬШИХ КВАДРАТОВ";
            g.DrawString(title, titleFont, Brushes.DarkBlue,
                drawingArea.Left + 10, drawingArea.Top + 5);

            // Функция
            if (!string.IsNullOrEmpty(function))
            {
                g.DrawString($"Функция: {function}", infoFont, Brushes.DarkRed,
                    drawingArea.Left + 10, drawingArea.Top + 30);
            }

            // Информация о данных
            if (points.Count > 0)
            {
                string dataInfo = $"Точек: {points.Count}, X: [{points.Min(p => p.X):F2}, {points.Max(p => p.X):F2}], Y: [{points.Min(p => p.Y):F2}, {points.Max(p => p.Y):F2}]";
                g.DrawString(dataInfo, infoFont, Brushes.DarkGreen,
                    drawingArea.Left + 10, drawingArea.Top + 50);
            }

            // Легенда
            string legend = "Легенда: ● - данные, — - аппроксимация, | - ошибка";
            g.DrawString(legend, new Font("Arial", 8), Brushes.DarkGray,
                graphArea.Left, graphArea.Bottom + 5);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }

}



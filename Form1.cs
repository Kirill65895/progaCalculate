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

        public Form1()
        {
            InitializeComponent();
            InitializeWindowComboBox();
            this.Text = "Калькулятор";
        }

        //*****************************************************************************| ТО ЧТО НЕ ВИДИТ ПОЛЬЗОВАТЕЛЬ И ЕМУ НЕ НАДО |***********************************************************************//

        // ФОРМА
        private void Form1_Load(object sender, EventArgs e)
        {

        }

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
                case 6: // Метод покоординатного спуска
                    InitializeCoordinateDescentControls();
                    break;
            }
        }

        // ПАНЕЛЬ 
        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {

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

        // ОТРИСОВКА СЕТКИ
        private void DrawGrid(Graphics g, Rectangle graphArea)
        {
            Pen gridPen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dot };

            // Вертикальные
            for (int x = graphArea.Left; x <= graphArea.Right; x += 20)
            {
                g.DrawLine(gridPen, x, graphArea.Top, x, graphArea.Bottom);
            }

            // Горизонтальные
            for (int y = graphArea.Top; y <= graphArea.Bottom; y += 20)
            {
                g.DrawLine(gridPen, graphArea.Left, y, graphArea.Right, y);
            }
        }

        // ОТРИСОВКА ОСЕЙ КООРДИНАТ
        private void DrawAxes(Graphics g, Rectangle graphArea)
        {
            Pen axisPen = new Pen(Color.Black, 2);

            // точка (0,0)
            PointF center = new PointF(
                graphArea.Left + graphArea.Width / 2,
                graphArea.Top + graphArea.Height / 2
            );

            // Ось X
            g.DrawLine(axisPen, graphArea.Left, center.Y, graphArea.Right, center.Y);

            // Стрелка оси X
            g.DrawLine(axisPen, graphArea.Right - 10, center.Y - 5, graphArea.Right, center.Y);
            g.DrawLine(axisPen, graphArea.Right - 10, center.Y + 5, graphArea.Right, center.Y);

            // Ось Y
            g.DrawLine(axisPen, center.X, graphArea.Top, center.X, graphArea.Bottom);

            // Стрелка оси Y
            g.DrawLine(axisPen, center.X - 5, graphArea.Top + 10, center.X, graphArea.Top);
            g.DrawLine(axisPen, center.X + 5, graphArea.Top + 10, center.X, graphArea.Top);
        }

        // ОТРИСОВКА ГРАФИКА ФУНКЦИИ
        private void DrawFunction(Graphics g, Rectangle graphArea, string functionStr)
        {
            try
            {
                // Масштаб: определяем диапазон отображения
                float scaleX = graphArea.Width / 20f; // Отображаем от -10 до 10 по X (20 единиц)
                float scaleY = graphArea.Height / 20f; // Отображаем от -10 до 10 по Y

                // Центр координат
                PointF center = new PointF(
                    graphArea.Left + graphArea.Width / 2,
                    graphArea.Top + graphArea.Height / 2
                );

                using (Pen graphPen = new Pen(Color.Blue, 2))
                using (GraphicsPath path = new GraphicsPath())
                {
                    List<PointF> points = new List<PointF>();
                    bool firstValidPoint = true;
                    PointF lastPoint = PointF.Empty;

                    // Количество точек для построения
                    int pointCount = graphArea.Width; // По одной точке на пиксель по ширине
                    double xStep = 20.0 / pointCount; // Шаг по X

                    for (int i = 0; i <= pointCount; i++)
                    {
                        double worldX = -10 + i * xStep;

                        try
                        {
                            double worldY = EvaluateMathExpression(functionStr, worldX);

                            // Преобразуем мировые координаты в экранные
                            float screenX = center.X + (float)(worldX * scaleX);
                            float screenY = center.Y - (float)(worldY * scaleY);

                            // Проверяем, не выходит ли точка за пределы области
                            if (screenY >= graphArea.Top - 100 && screenY <= graphArea.Bottom + 100)
                            {
                                PointF currentPoint = new PointF(screenX, screenY);

                                if (firstValidPoint)
                                {
                                    firstValidPoint = false;
                                    lastPoint = currentPoint;
                                }
                                else
                                {
                                    // Проверяем разрыв функции
                                    if (Math.Abs(currentPoint.Y - lastPoint.Y) < graphArea.Height * 2)
                                    {
                                        path.AddLine(lastPoint, currentPoint);
                                    }
                                    else
                                    {
                                        // Если разрыв большой, начинаем новый сегмент
                                        firstValidPoint = true;
                                    }
                                }
                                lastPoint = currentPoint;
                            }
                        }
                        catch
                        {
                            // Точка не определена, начинаем новый сегмент
                            firstValidPoint = true;
                        }
                    }

                    // Рисуем график
                    g.DrawPath(graphPen, path);

                    // Если был найден корень, отмечаем его
                    if (lastRoot != 0)
                    {
                        float rootX = center.X + (float)(lastRoot * scaleX);
                        float rootY = center.Y;

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
                throw new Exception($"Ошибка построения графика: {ex.Message}");
            }
        }

        // ОТРИСОВКА ПОДПИСЕЙ ОСЕЙ И ДЕЛЕНИЙ
        private void DrawLabels(Graphics g, Rectangle graphArea, Rectangle drawingArea)
        {
            Font labelFont = new Font("Arial", 9);
            Brush labelBrush = Brushes.Black;

            // Центр 
            PointF center = new PointF(
                graphArea.Left + graphArea.Width / 2,
                graphArea.Top + graphArea.Height / 2
            );

            // Подпись оси X
            g.DrawString("X", new Font("Arial", 10, FontStyle.Bold), Brushes.Black,
                drawingArea.Right - 20, center.Y + 5);

            // Подпись оси Y
            g.DrawString("Y", new Font("Arial", 10, FontStyle.Bold), Brushes.Black,
                center.X - 20, drawingArea.Top + 5);

            // Подписи делений на оси X
            for (int i = -10; i <= 10; i += 2)
            {
                if (i == 0) continue; // Пропускаем 0, чтобы не накладывалось на ось 

                float x = center.X + i * (graphArea.Width / 20f);
                g.DrawLine(Pens.Black, x, center.Y - 3, x, center.Y + 3);
                g.DrawString(i.ToString(), labelFont, labelBrush, x - 10, center.Y + 5);
            }

            // Подписи делений на оси Y
            for (int i = -10; i <= 10; i += 2)
            {
                if (i == 0) continue; // Пропускаем 0, чтобы не накладывалось на ось X

                float y = center.Y - i * (graphArea.Height / 20f);
                g.DrawLine(Pens.Black, center.X - 3, y, center.X + 3, y);
                g.DrawString(i.ToString(), labelFont, labelBrush, center.X - 25, y - 7);
            }

            // Начало координат
            g.DrawString("0", labelFont, labelBrush, center.X + 3, center.Y + 3);

            // Подпись функции
            if (!string.IsNullOrEmpty(currentFunction))
            {
                g.DrawString($"f(x) = {currentFunction}",
                    new Font("Arial", 10, FontStyle.Bold), Brushes.DarkBlue,
                    graphArea.Left, drawingArea.Top + 5);
            }
        }

        // РАСЧЕТЫ МЕТОДА ДИХОТОМИИ
        private void CalculateDichotomy(string functionStr, string aStr, string bStr, string epsilonStr, Label resultLabel)
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
                // Создаем парсер
                var parser = new MathParser();

                // Добавляем переменные
                parser.LocalVariables["x"] = x;
                parser.LocalVariables["pi"] = Math.PI;
                parser.LocalVariables["e"] = Math.E;

                // Предварительная обработка выражения
                expression = PreprocessExpression(expression);

                // Парсим и вычисляем выражение
                return parser.Parse(expression);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Ошибка вычисления выражения '{expression}': {ex.Message}");
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

            var btnFindZero = new System.Windows.Forms.Button
            {
                Text = "Найти ноль (f(x)=0)",
                Location = new Point(10, 190),
                BackColor = Color.LightGreen,
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

            btnFindZero.Click += (s, e) => FindZero(
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
        btnFindMin, btnFindMax, btnFindZero,
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
                Text = "Вычислить корень",
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

        // МЕТОД НЬЮТОНА ДЛЯ НАХОЖДЕНИЯ КОРНЯ
        private void CalculateNewtonMethod(string functionStr, string derivativeStr, string x0Str, string epsilonStr, string maxIterStr, Label resultLabel, DataGridView dataGridView)
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
                double fpxn = 0;
                double delta = 0;
                int iteration = 0;
                bool converged = false;

                List<double> iterationPoints = new List<double>();
                iterationPoints.Add(xn);

                // Сохраняем для отрисовки
                currentFunction = functionStr;
                lastRoot = 0;

                // Основной цикл метода Ньютона
                while (iteration < maxIterations)
                {
                    // Вычисляем значение функции
                    fxn = EvaluateMathExpression(functionStr, xn);

                    // Вычисляем производную
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

                    // Проверка на нулевую производную
                    if (Math.Abs(fpxn) < accuracy)
                    {
                        resultLabel.Text = $"Ошибка: Производная близка к нулю!\n" +
                                          $"f'({xn:F6}) = {fpxn:E}\n" +
                                          $"Итерация: {iteration}";
                        return;
                    }

                    // Формула метода Ньютона: x_{n+1} = x_n - f(x_n)/f'(x_n)
                    double xn1 = xn - fxn / fpxn;
                    delta = Math.Abs(xn1 - xn);

                    // Добавляем строку в таблицу
                    dataGridView.Rows.Add(iteration + 1,
                                         xn.ToString("F6"),
                                         fxn.ToString("E4"),
                                         fpxn.ToString("E4"),
                                         delta.ToString("E4"));

                    // Сохраняем точку для отрисовки
                    iterationPoints.Add(xn1);

                    // Проверка условия остановки
                    if (Math.Abs(fxn) < accuracy || delta < accuracy)
                    {
                        converged = true;
                        xn = xn1;
                        lastRoot = xn;
                        break;
                    }

                    xn = xn1;
                    iteration++;
                }

                // Формируем результат
                if (converged)
                {
                    fxn = EvaluateMathExpression(functionStr, xn);
                    resultLabel.Text = $"Корень найден:\n" +
                                      $"x = {xn:F8}\n" +
                                      $"f(x) = {fxn:E}\n" +
                                      $"Итераций: {iteration + 1}\n" +
                                      $"Точность: {epsilon}";

                    lastRoot = xn;
                }
                else
                {
                    resultLabel.Text = $"Метод не сошелся за {maxIterations} итераций!\n" +
                                      $"Последнее приближение: {xn:F8}\n" +
                                      $"f(x) = {fxn:E}\n" +
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

        // ОТРИСОВКА КАСАТЕЛЬНЫХ ДЛЯ ИТЕРАЦИЙ МЕТОДА НЬЮТОНА
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
                    // Вычисляем две точки для отрисовки линии
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

                    // Рисуем вертикальную линию к оси X для следующего приближения
                    if (i < newtonIterationPoints.Count - 1)
                    {
                        double nextX = newtonIterationPoints[i + 1];
                        float nextScreenX = center.X + (float)(nextX * scaleX);

                        using (Pen guidePen = new Pen(Color.Gray, 1f))
                        {
                            guidePen.DashStyle = DashStyle.Dot;
                            g.DrawLine(guidePen, pointX, pointY, nextScreenX, pointY);
                            g.DrawLine(guidePen, nextScreenX, pointY, nextScreenX, center.Y);
                        }
                    }
                }
                catch { }
            }

            // Рисуем последний найденный корень
            if (lastRoot != 0)
            {
                float rootX = center.X + (float)(lastRoot * scaleX);

                // Зеленый кружок на оси X
                g.FillEllipse(Brushes.Green, rootX - 5, center.Y - 5, 10, 10);
                g.DrawEllipse(Pens.DarkGreen, rootX - 5, center.Y - 5, 10, 10);

                g.DrawString($"Корень: {lastRoot:F4}",
                    new Font("Arial", 9, FontStyle.Bold), Brushes.DarkGreen,
                    rootX + 5, center.Y - 15);
            }
        }

        // ПОДПИСИ ДЛЯ ГРАФИКА МЕТОДА НЬЮТОНА
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
                string title = $"Метод Ньютона: f(x) = {functionStr}";
                SizeF titleSize = g.MeasureString(title, titleFont);
                g.DrawString(title, titleFont, Brushes.DarkBlue,
                    drawingArea.Left + 10, drawingArea.Top + 10);
            }

            // Легенда
            string legend = "Обозначения:  ● - итерации  --- - касательные  ● - найденный корень";
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

        // ПОСТРОЕНИЕ ГРАФИКА ИНТЕГРАЛА
        private void DrawIntegralGraph(Panel panel, string functionStr, string aStr, string bStr)
        {
            try
            {
                double a = double.Parse(aStr);
                double b = double.Parse(bStr);

                panel.Paint += (sender, e) =>
                {
                    DrawIntegralFunction(e.Graphics, panel.ClientRectangle, functionStr, a, b);
                };

                panel.Invalidate(); // Перерисовываем
            }
            catch
            {
                // Игнорируем ошибки
            }
        }

        // ОТРИСОВКА ГРАФИКА ИНТЕГРИРУЕМОЙ ФУНКЦИИ
        private void DrawIntegralFunction(Graphics g, Rectangle drawingArea, string functionStr, double a, double b)
        {
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int padding = 30;
            Rectangle graphArea = new Rectangle(
                drawingArea.Left + padding,
                drawingArea.Top + padding,
                drawingArea.Width - 2 * padding,
                drawingArea.Height - 2 * padding
            );

            // Сетка и оси
            DrawGrid(g, graphArea);
            DrawAxes(g, graphArea);

            // Вычисляем диапазон значений функции
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            int samples = 100;

            for (int i = 0; i <= samples; i++)
            {
                double x = a + (b - a) * i / samples;
                try
                {
                    double y = EvaluateMathExpression(functionStr, x);
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);
                }
                catch { }
            }

            // Добавляем немного места сверху и снизу
            double rangeY = maxY - minY;
            minY -= rangeY * 0.1;
            maxY += rangeY * 0.1;

            // Масштаб
            float scaleX = graphArea.Width / (float)(b - a);
            float scaleY = graphArea.Height / (float)(maxY - minY);

            // Центр координат
            PointF origin = new PointF(
                graphArea.Left - (float)(a * scaleX),
                graphArea.Bottom + (float)(minY * scaleY)
            );

            // Рисуем функцию
            using (Pen graphPen = new Pen(Color.Blue, 2))
            {
                PointF? lastPoint = null;

                for (int i = 0; i <= graphArea.Width; i++)
                {
                    double x = a + (b - a) * i / graphArea.Width;

                    try
                    {
                        double y = EvaluateMathExpression(functionStr, x);

                        float screenX = origin.X + (float)(x * scaleX);
                        float screenY = origin.Y - (float)(y * scaleY);

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

            // Закрашиваем область под кривой (интеграл)
            using (Brush integralBrush = new SolidBrush(Color.FromArgb(100, Color.LightBlue)))
            {
                List<PointF> points = new List<PointF>();

                // Начинаем с левого нижнего угла
                points.Add(new PointF(origin.X + (float)(a * scaleX), origin.Y));

                // Добавляем точки функции
                for (int i = 0; i <= graphArea.Width; i++)
                {
                    double x = a + (b - a) * i / graphArea.Width;
                    try
                    {
                        double y = EvaluateMathExpression(functionStr, x);
                        float screenX = origin.X + (float)(x * scaleX);
                        float screenY = origin.Y - (float)(y * scaleY);
                        points.Add(new PointF(screenX, screenY));
                    }
                    catch { }
                }

                // Заканчиваем правым нижним углом
                points.Add(new PointF(origin.X + (float)(b * scaleX), origin.Y));

                // Рисуем заполненную область
                if (points.Count > 2)
                {
                    g.FillPolygon(integralBrush, points.ToArray());
                }
            }

            // Подписи
            Font labelFont = new Font("Arial", 9);
            g.DrawString($"∫f(x)dx на [{a:F2}, {b:F2}]",
                new Font("Arial", 10, FontStyle.Bold), Brushes.DarkBlue,
                graphArea.Left, drawingArea.Top + 5);

            g.DrawString($"f(x) = {functionStr}", labelFont, Brushes.Black,
                graphArea.Left, drawingArea.Top + 25);
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

        //*************************************************************************************| МЕТОД ПОКООРДИНАТНОГО СПУСКА |*******************************************************************************//

        // ИНТЕРФЕЙС
        private void InitializeCoordinateDescentControls()
        {
            // Очищаем панель
            panel1.Controls.Clear();
        }
    }

}



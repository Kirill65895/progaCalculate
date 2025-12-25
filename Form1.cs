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
                case 6: // Решение СЛАУ
                    InitializeSloughSolutionControls();
                    break;
                case 7: // Метод покоординатного спуска
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

        // МЕТОД ДЛЯ ВЫЧИСЛЕНИЯ ПРОИЗВОДНОЙ (упрощенный)
        private string CalculateDerivative(string function)
        {
            // Упрощенный метод вычисления производной (для основных функций)
            // В реальном проекте лучше использовать символьные вычисления

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
        }

        //********************************************************************************************| РЕШЕНИЕ СЛАУ |****************************************************************************************//

        // ИНТЕРФЕЙС (РЕШЕНИЯ СЛАУ)
        private void InitializeSloughSolutionControls()
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
        }
    }

}



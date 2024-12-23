using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EulerSimulation
{
    public partial class MainWindow : Window
    {
        // Simulation parameters
        const int GridSize = 100; // Example: 100x100 grid
        double[,] rho = new double[GridSize, GridSize]; // Mass density
        // double[,] S = new double[GridSize, GridSize];   // Specific entropy density
        Vector2[,] j = new Vector2[GridSize, GridSize]; // Momentum
        double[,] pressure = new double[GridSize, GridSize]; // Pressure
        Vector2[,] externalForce = new Vector2[GridSize, GridSize]; // External force
        double[,] b = new double[GridSize, GridSize]; // for Poisson Equation used for calculating pressure

        // Time step and grid spacing
        double dt = 0.001;
        double dx = 1;

        // Visualization variables
        private Rectangle[,] densityCells = new Rectangle[GridSize, GridSize];

        // Timer for simulation loop
        DispatcherTimer timer;

        // Constructor
        public MainWindow()
        {
            InitializeComponent();
            InitializeGrid();
            InitializeSimulation();
        }

        // Initialize grid visualization
        private void InitializeGrid()
        {
            double cellWidth = SimulationCanvas.Width / GridSize;
            double cellHeight = SimulationCanvas.Height / GridSize;

            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    var rect = new Rectangle
                    {
                        Width = cellWidth,
                        Height = cellHeight,
                        Fill = Brushes.White,
                        Stroke = Brushes.Black,
                        StrokeThickness = 0.5
                    };
                    Canvas.SetLeft(rect, i * cellWidth);
                    Canvas.SetTop(rect, j * cellHeight);
                    SimulationCanvas.Children.Add(rect);
                    densityCells[i, j] = rect;
                }
            }
        }

        // Initialize simulation variables
        private void InitializeSimulation()
        {
            // Set initial values for rho
            Random random = new Random();
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    rho[i, j] = 1.225; // Uniform density
                    // S[i, j] = 3796.0; // Uniform entropy
                    this.j[i, j] = new Vector2(10, 0); // Zero initial momentum
                    pressure[i, j] = 101.3; // Uniform pressure
                    externalForce[i, j] = new Vector2(0, 9.81); // Gravity: (0, 9.81)
                    b[i, j] = 0;
                }
            }
            // Outside boundary
            for (int i = 0; i < GridSize; i++)
            {
                this.j[i, 0] = new Vector2(0, 0);
                this.j[0, i] = new Vector2(0, 0);
                this.j[i, GridSize - 1] = new Vector2(0, 0);
                this.j[GridSize - 1, i] = new Vector2(0, 0);
                pressure[i, 0] = 0;
                pressure[0, i] = 0;
                pressure[i, GridSize - 1] = 0;
                pressure[GridSize - 1, i] = 0;
                externalForce[i, 0] = new Vector2(0, 0);
                externalForce[0, i] = new Vector2(0, 0);
                externalForce[i, GridSize - 1] = new Vector2(0, 0);
                externalForce[GridSize - 1, i] = new Vector2(0, 0);
                //b[0, i] = 1000.0;
                //b[i, 0] = 1000.0;
                //b[GridSize - 1, i] = 1000.0;
                //b[i, GridSize - 1] = 1000.0;
            }
            /*
            for (int i = 1; i < GridSize - 1; i++)
            {
                for (int j = 1; j < GridSize - 1; j++)
                {
                    rho[i, j] = random.NextDouble() * 0.05 + 1.2; // Random density
                }
            }
            */
            for (int i = 50; i < GridSize - 1; i++)
                for (int j = 120 - i; j < GridSize - 1; j++)
                {
                    pressure[j, i] = 99.6;
                    rho[j, i] = 1.25;
                    pressure[j, GridSize - 1 - i] = 99.6;
                    rho[j, GridSize - 1 - i] = 1.25;
                    this.j[j, i] = new Vector2(-10, 0);
                    this.j[j, GridSize - 1 - i] = new Vector2(-10, 0);
                }

            for (int i = 50; i < GridSize - 1; i++)
            {
                rho[120 - i, i] = 1.25;
                rho[120 - i, GridSize - 1 - i] = 1.25;
            }

            // Timer for updates
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Update dt seconds per 1 second
            };
            timer.Tick += UpdateSimulation;
        }

        // Start simulation
        private void StartSimulation(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        // Stop simulation
        private void StopSimulation(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        // Update simulation
        private void UpdateSimulation(object sender, EventArgs e)
        {
            Vector2[,] nj = new Vector2[GridSize, GridSize];
            Double[,] np = new Double[GridSize, GridSize];
            Double[,] nrho = new Double[GridSize, GridSize];
            // Double[,] ns = new Double[GridSize, GridSize];
            // Calculate new states
            // Calculate new pressure based on Poisson's Equation
            // Calculate b
            for (int i = 0; i < GridSize - 1; i++)
            {
                for (int j = 0; j < GridSize - 1; j++)
                {
                    b[i, j] = rho[i, j] * 
                            (Math.Pow(this.j[i + 1, j].X / rho[i + 1, j] - this.j[i, j].X / rho[i, j], 2)
                             + 2 * ((this.j[i, j + 1].X / rho[i, j + 1]) - (this.j[i, j].X / rho[i, j]))
                             * ((this.j[i + 1, j].Y / rho[i + 1, j]) - (this.j[i, j].Y / rho[i, j]))
                             + Math.Pow(this.j[i, j + 1].Y / rho[i, j + 1] - this.j[i, j].Y / rho[i, j], 2));
                }
            }
            // Calculate pressure for 100 iterations until it reaches quasilinear state
            for (int z = 0; z < 100; z++)
            {
                for (int i = 1; i < GridSize - 1; i++)
                {
                    for (int j = 1; j < GridSize - 1; j++)
                    {
                        np[i, j] = (pressure[i + 1, j] + pressure[i - 1, j]
                                    + pressure[i, j + 1] + pressure[i, j - 1] + b[i, j] * dx) / 4.0;
                    }
                }
                // Update pressure
                for (int i = 1; i < GridSize - 1; i++)
                {
                    for (int j = 1; j < GridSize - 1; j++)
                    {
                        pressure[i, j] = np[i, j];
                    }
                }
            }
            // Calculate new j by Eulerian form of Euler Equations using RK4 method for more accurate approximation
            Vector2[,] m = new Vector2[GridSize, GridSize];
            Vector2[,] m1 = new Vector2[GridSize, GridSize];
            Vector2[,] m2 = new Vector2[GridSize, GridSize];
            Vector2[,] m3 = new Vector2[GridSize, GridSize];
            Vector2[,] m4 = new Vector2[GridSize, GridSize];
            // Calculate m1 = f(tn, xn), nj = xn + m1 * h / 2
            for (int i = 0; i < GridSize - 1; i++)
            {
                for (int j = 0; j < GridSize - 1; j++)
                {
                    double flux_x = Math.Pow(this.j[i + 1, j].X, 2) / rho[i + 1, j]
                                  + this.j[i, j + 1].X * this.j[i, j + 1].Y / rho[i, j + 1]
                                  - Math.Pow(this.j[i, j].X, 2) / rho[i, j]
                                  - this.j[i, j].X * this.j[i, j].Y / rho[i, j]
                                  + pressure[i + 1, j] - pressure[i, j];
                    double flux_y = Math.Pow(this.j[i, j + 1].Y, 2) / rho[i, j + 1]
                                  + this.j[i + 1, j].X * this.j[i + 1, j].Y / rho[i + 1, j]
                                  - Math.Pow(this.j[i, j].Y, 2) / rho[i, j]
                                  - this.j[i, j].X * this.j[i, j].Y / rho[i, j]
                                  + pressure[i, j + 1] - pressure[i, j];
                    m1[i, j].X = externalForce[i, j].X * rho[i, j] - flux_x / dx;
                    m1[i, j].Y = externalForce[i, j].Y * rho[i, j] - flux_y / dx;
                    nj[i, j].X = this.j[i, j].X + m1[i, j].X * dt / 2;
                    nj[i, j].Y = this.j[i, j].Y + m1[i, j].Y * dt / 2;
                    /* nrho[i, j] = rho[i, j] - dt / dx * (this.j[i + 1, j].X + this.j[i, j + 1].Y
                                                        - this.j[i, j].X - this.j[i, j].Y);
                    /* ns[i, j] = S[i, j] - dt / dx * (S[i + 1, j] * this.j[i + 1, j].X / rho[i + 1, j]
                                                    + S[i, j + 1] * this.j[i, j + 1].Y / rho[i, j + 1]
                                                    - S[i, j] * this.j[i, j].X / rho[i, j]
                                                    - S[i, j] * this.j[i, j].Y / rho[i, j]);*/
                }
            }
            // Calculate m2 = f(tn + h / 2, xn + m1 * h / 2), nj = xn + m2 * h / 2
            for (int i = 0; i < GridSize - 1; i++)
            {
                for (int j = 0; j < GridSize - 1; j++)
                {
                    double flux_x = Math.Pow(nj[i + 1, j].X, 2) / rho[i + 1, j]
                                  + nj[i, j + 1].X * nj[i, j + 1].Y / rho[i, j + 1]
                                  - Math.Pow(nj[i, j].X, 2) / rho[i, j]
                                  - nj[i, j].X * nj[i, j].Y / rho[i, j]
                                  + pressure[i + 1, j] - pressure[i, j];
                    double flux_y = Math.Pow(nj[i, j + 1].Y, 2) / rho[i, j + 1]
                                  + nj[i + 1, j].X * nj[i + 1, j].Y / rho[i + 1, j]
                                  - Math.Pow(nj[i, j].Y, 2) / rho[i, j]
                                  - nj[i, j].X * nj[i, j].Y / rho[i, j]
                                  + pressure[i, j + 1] - pressure[i, j];
                    m2[i, j].X = externalForce[i, j].X * rho[i, j] - flux_x / dx;
                    m2[i, j].Y = externalForce[i, j].Y * rho[i, j] - flux_y / dx;
                }
            }
            for (int i = 0; i < GridSize - 1; i++)
            {
                for (int j = 0; j < GridSize - 1; j++)
                {
                    nj[i, j].X = this.j[i, j].X + m2[i, j].X * dt / 2;
                    nj[i, j].Y = this.j[i, j].Y + m2[i, j].Y * dt / 2;
                }
            }
            // Calculate m3 = f(tn + h / 2, xn + m2 * h / 2), nj = xn + m3 * h
            for (int i = 0; i < GridSize - 1; i++)
            {
                for (int j = 0; j < GridSize - 1; j++)
                {
                    double flux_x = Math.Pow(nj[i + 1, j].X, 2) / rho[i + 1, j]
                                  + nj[i, j + 1].X * nj[i, j + 1].Y / rho[i, j + 1]
                                  - Math.Pow(nj[i, j].X, 2) / rho[i, j]
                                  - nj[i, j].X * nj[i, j].Y / rho[i, j]
                                  + pressure[i + 1, j] - pressure[i, j];
                    double flux_y = Math.Pow(nj[i, j + 1].Y, 2) / rho[i, j + 1]
                                  + nj[i + 1, j].X * nj[i + 1, j].Y / rho[i + 1, j]
                                  - Math.Pow(nj[i, j].Y, 2) / rho[i, j]
                                  - nj[i, j].X * nj[i, j].Y / rho[i, j]
                                  + pressure[i, j + 1] - pressure[i, j];
                    m3[i, j].X = externalForce[i, j].X * rho[i, j] - flux_x / dx;
                    m3[i, j].Y = externalForce[i, j].Y * rho[i, j] - flux_y / dx;
                }
            }
            for (int i = 0; i < GridSize - 1; i++)
            {
                for (int j = 0; j < GridSize - 1; j++)
                {
                    nj[i, j].X = this.j[i, j].X + m3[i, j].X * dt;
                    nj[i, j].Y = this.j[i, j].Y + m3[i, j].Y * dt;
                }
            }
            // Calculate m4 = f(tn + h, xn + m3 * h)
            for (int i = 0; i < GridSize - 1; i++)
            {
                for (int j = 0; j < GridSize - 1; j++)
                {
                    double flux_x = Math.Pow(nj[i + 1, j].X, 2) / rho[i + 1, j]
                                  + nj[i, j + 1].X * nj[i, j + 1].Y / rho[i, j + 1]
                                  - Math.Pow(nj[i, j].X, 2) / rho[i, j]
                                  - nj[i, j].X * nj[i, j].Y / rho[i, j]
                                  + pressure[i + 1, j] - pressure[i, j];
                    double flux_y = Math.Pow(nj[i, j + 1].Y, 2) / rho[i, j + 1]
                                  + nj[i + 1, j].X * nj[i + 1, j].Y / rho[i + 1, j]
                                  - Math.Pow(nj[i, j].Y, 2) / rho[i, j]
                                  - nj[i, j].X * nj[i, j].Y / rho[i, j]
                                  + pressure[i, j + 1] - pressure[i, j];
                    m4[i, j].X = externalForce[i, j].X * rho[i, j] - flux_x / dx;
                    m4[i, j].Y = externalForce[i, j].Y * rho[i, j] - flux_y / dx;
                }
            }
            // Calculate m = (1 / 6) * (m1 + 2 * m2 + 2 * m3 + m4)
            for (int i = 0; i < GridSize - 1; i++)
            {
                for (int j = 0; j < GridSize - 1; j++)
                {
                    m[i, j].X = m1[i, j].X / 6 + m2[i, j].X / 3 + m3[i, j].X / 3 + m4[i, j].X / 6;
                    m[i, j].Y = m1[i, j].Y / 6 + m2[i, j].Y / 3 + m3[i, j].Y / 3 + m4[i, j].Y / 6;
                }
            }
            // Calculate x(n+1) = x(n) + h * m, update j
            for (int i = 0; i < GridSize - 1; i++)
            {
                for (int j = 0; j < GridSize - 1; j++)
                {
                    nj[i, j].X = this.j[i, j].X + m[i, j].X * dt;
                    nj[i, j].Y = this.j[i, j].Y + m[i, j].Y * dt;
                    this.j[i, j].X = nj[i, j].X;
                    this.j[i, j].Y = nj[i, j].Y;
                }
            }
            // Use the updated j to calculate and update rho
            for (int i = 0; i < GridSize - 1; i++)
            {
                for (int j = 0; j < GridSize - 1; j++)
                {
                    nrho[i, j] = rho[i, j] - (this.j[i + 1, j].X + this.j[i, j + 1].Y
                                           - this.j[i, j].X - this.j[i, j].Y) / dx * dt;
                    rho[i, j] = nrho[i, j];
                }
            }
            /*
             * for (int i = 1; i < GridSize - 1; i++)
            {
                for (int j = 1; j < GridSize - 1; j++)
                {
                    double flux_x = Math.Pow(this.j[i + 1, j].X, 2) / rho[i + 1, j]
                                  + this.j[i, j + 1].X * this.j[i, j + 1].Y / rho[i, j + 1]
                                  - Math.Pow(this.j[i - 1, j].X, 2) / rho[i - 1, j]
                                  - this.j[i, j - 1].X * this.j[i, j - 1].Y / rho[i, j - 1]
                                  + pressure[i + 1, j] - pressure[i - 1, j];
                    double flux_y = Math.Pow(this.j[i, j + 1].Y, 2) / rho[i, j + 1]
                                  + this.j[i + 1, j].X * this.j[i + 1, j].Y / rho[i + 1, j]
                                  - Math.Pow(this.j[i, j - 1].Y, 2) / rho[i, j - 1]
                                  - this.j[i - 1, j].X * this.j[i - 1, j].Y / rho[i - 1, j]
                                  + pressure[i, j + 1] - pressure[i, j - 1];
                    np[i, j] = (pressure[i + 1, j] + pressure[i - 1, j] + pressure[i, j + 1] + pressure[i, j - 1] - b[i, j] * dx) / 4.0;
                    nj[i, j].X = this.j[i, j].X + externalForce[i, j].X * dt - dt / dx * flux_x / 2;
                    nj[i, j].Y = this.j[i, j].Y + externalForce[i, j].Y * dt - dt / dx * flux_y / 2;
                    nrho[i, j] = rho[i, j] - dt / dx * (this.j[i + 1, j].X + this.j[i, j + 1].Y
                                                        - this.j[i - 1, j].X - this.j[i, j - 1].Y) / 2;
                    ns[i, j] = S[i, j] - dt / dx * (S[i + 1, j] * this.j[i + 1, j].X / rho[i + 1, j]
                                                    + S[i, j + 1] * this.j[i, j + 1].Y / rho[i, j + 1]
                                                    - S[i - 1, j] * this.j[i - 1, j].X / rho[i - 1, j]
                                                    - S[i, j - 1] * this.j[i, j - 1].Y / rho[i, j - 1]) / 2;
                }
            }
             */
            /* Update states
            for (int i = 1; i < GridSize - 1; i++)
            {
                for (int j = 1; j < GridSize - 1; j++)
                {
                    this.j[i, j] = nj[i, j];
                    pressure[i, j] = np[i, j];
                    rho[i, j] = nrho[i, j];
                    // S[i, j] = ns[i, j];
                }
            }*/
            // Update visualization
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    double value = rho[i, j];
                    densityCells[i, j].Fill = new SolidColorBrush(ColorFromValue(value));
                }
            }
        }

        // Vector2 structure
        public struct Vector2
        {
            public double X, Y;
            public Vector2(double x, double y) { X = x; Y = y; }

            // Vector addition
            public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);

            // Vector scalar division
            public static Vector2 operator /(Vector2 a, double scalar) => new Vector2(a.X / scalar, a.Y / scalar);

            // Vector scalar multiplication
            public static Vector2 operator *(Vector2 a, double scalar) => new Vector2(a.X * scalar, a.Y * scalar);
        }

        // Map value to color
        private Color ColorFromValue(double value)
        {
            byte intensity = (byte)(255 - 255 * Clamp(value, 1.2, 1.3));
            return Color.FromRgb(intensity, intensity, 255); // Blue gradient
        }
        public static double Clamp(double value, double min, double max)
        {
            if (value <= min)
            {
                return 0.0;
            }
            else if (value >= max)
            {
                return 1.0;
            }

            return (value - min) / (max - min);
        }
    }
}

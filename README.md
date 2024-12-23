v0.0.1-alpha

Side Project. Built with C# under .Net 9.0 by Visual Studio 2022. A Simulation of 2D Euler Equations based on 1st Order Finite Difference Method. Variables need to be manually modified and recompiled. Uses Poisson's Equation for pressure calculation.

v0.0.2-alpha

Side Project. Built with C# under .Net 9.0 by Visual Studio 2022. A Simulation of 2D Euler Equations based on Finite Difference Method, the momentum density J is simulated using RK4 method for better accuracy over time. Variables need to be manually modified and recompiled. Uses Poisson's Equation for pressure calculation.

Updates:

v0.0.3-alpha-fix

Fixes major bug on Poisson's Equation part in the program for accurate pressure calculation. Now there are 100 iterations for each dt seconds to ensure that the pressure reaches pseudo-quasilinear state before proceeding into the calculation of J and $\rho$. I also fix the error on the calculation of b. The code should now work fine for incompressible fluids without shock waves, a feature lacking for Finite Difference Methods. As you see in the demo, the pressure builds up and initiates a shock-like action in the middle of the shaped edge. And because of the lacking nature of Finite Difference Methods on simulating results with close-to-discontinuous waves, the error on approximating density and pressure builds up exponentially, causing a all-blue screen at the end, which violates the law of conservation and, of course, does not occur in real life. 

Ready to do:

1. Better outer boundary conditions
2. Add entropy calculation for pressure calculation for better simulation on compressible fluids
3. Adjust the project for Finite Volume Method for more accurate results that fit in thermodynamics as well as good results with discontinuous fluids

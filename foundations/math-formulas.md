# Factorial
The factorial of a non-negative integer n, denoted as n!, is the product of all positive integers less than or equal to n. The factorial formula is given by:

n! = n × (n - 1) × (n - 2) × ... × 3 × 2 × 1

For example:

    0! = 1 (by convention)
    1! = 1
    2! = 2 × 1 = 2
    3! = 3 × 2 × 1 = 6
    4! = 4 × 3 × 2 × 1 = 24
    5! = 5 × 4 × 3 × 2 × 1 = 120

The factorial function can be defined recursively as follows:

n! = n × (n - 1)!  if n > 0

0! = 1  (base case)

Here's the factorial formula represented mathematically:

n! = { 

       1                    if n = 0

       n × (n - 1)!         if n > 0

     }

The factorial function grows very rapidly, and the value of n! becomes extremely large even for relatively small values of n. For example, 20! = 2,432,902,008,176,640,000, which is a 21-digit number.

Factorials have various applications in mathematics, combinatorics, probability theory, and other areas of science and engineering. They are often used to calculate permutations, combinations, and other counting problems.

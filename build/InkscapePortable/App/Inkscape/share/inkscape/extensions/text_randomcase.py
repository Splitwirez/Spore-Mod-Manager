#!/usr/bin/env python
"""Randomise the case of the letters."""

import random
import inkex

class RandomCase(inkex.TextExtension):
    """Randomise the case of the text (with bias)"""
    previous_case = 1

    def map_char(self, char):
        # bias the randomness towards inversion of the previous case:
        # We use this weird way to get from a random set because
        # python2 and python3 have different ways of seeding
        if self.previous_case > 0:
            case = [-2, -1, 1][int(random.random() * 3)]
        else:
            case = [-1, 1, 2][int(random.random() * 3)]

        if char.isalpha():
            self.previous_case = case
            if case > 0:
                return char.upper()
            elif case < 0:
                return char.lower()
        return char

if __name__ == '__main__':
    RandomCase().run()

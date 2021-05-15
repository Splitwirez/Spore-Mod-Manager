#!/usr/bin/env python
"""Convert to sentence case"""

import inkex

class SentenceCase(inkex.TextExtension):
    """Convert text to sentence case"""
    sentence_start = True
    was_punctuation = False

    def map_char(self, char):
        """Turn the char into a sentence using class state"""
        if char in '.!?':
            self.was_punctuation = True
        elif ((char.isspace() or self.newline) and self.was_punctuation) or self.newpar:
            self.sentence_start = True
            self.was_punctuation = False
        elif char in '")':
            pass
        else:
            self.was_punctuation = False

        if not char.isspace():
            self.newline = False
            self.newpar = False

        if self.sentence_start and char.isalpha():
            self.sentence_start = False
            return char.upper()
        elif not self.sentence_start and char.isalpha():
            return char.lower()
        return char

if __name__ == '__main__':
    SentenceCase().run()

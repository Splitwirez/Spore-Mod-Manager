#!/usr/bin/env python
# -*- coding: UTF-8 -*-
#
# Copyright (C) 2009 John Beard john.j.beard@gmail.com
#
# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
#
"""
This extension renders a DataMatrix 2D barcode, as specified in
BS ISO/IEC 16022:2006. Only ECC200 codes are considered, as these are the only
ones recommended for an "open" system.

The size of the DataMatrix is variable between 10x10 to 144x144

The absolute size of the DataMatrix modules (the little squares) is also
variable.

If more data is given than can be contained in one DataMatrix,
more than one DataMatrices will be produced.

Text is encoded as ASCII (the standard provides for other options, but these are
not implemented). Consecutive digits are encoded in a compressed form, halving
the space required to store them.

The basis processing flow is;
    * Convert input string to codewords (modified ASCII and compressed digits)
    * Split codewords into blocks of the right size for Reed-Solomon coding
    * Interleave the blocks if required
    * Apply Reed-Solomon coding
    * De-interleave the blocks if required
    * Place the codewords into the matrix bit by bit
    * Render the modules in the matrix as squares
"""

import inkex
from inkex import Rectangle

INVALID_BIT = 2

# return parameters for the selected datamatrix size
#   drow        number of rows in each data region
#   dcol        number of cols in each data region
#   reg_row     number of rows of data regions
#   reg_col     number of cols of data regions
#   nd          number of data codewords per reed-solomon block
#   nc          number of ECC codewords per reed-solomon block
#   inter       number of interleaved Reed-Solomon blocks
SYMBOLS = {
    # 'id': (nrow, ncol, drow, dcol, reg_row, reg_col, nd, nc, inter)
    'sq10': (10, 10, 8, 8, 1, 1, 3, 5, 1),
    'sq12': (12, 12, 10, 10, 1, 1, 5, 7, 1),
    'sq14': (14, 14, 12, 12, 1, 1, 8, 10, 1),
    'sq16': (16, 16, 14, 14, 1, 1, 12, 12, 1),
    'sq18': (18, 18, 16, 16, 1, 1, 18, 14, 1),
    'sq20': (20, 20, 18, 18, 1, 1, 22, 18, 1),
    'sq22': (22, 22, 20, 20, 1, 1, 30, 20, 1),
    'sq24': (24, 24, 22, 22, 1, 1, 36, 24, 1),
    'sq26': (26, 26, 24, 24, 1, 1, 44, 28, 1),
    'sq32': (32, 32, 14, 14, 2, 2, 62, 36, 1),
    'sq36': (36, 36, 16, 16, 2, 2, 86, 42, 1),
    'sq40': (40, 40, 18, 18, 2, 2, 114, 48, 1),
    'sq44': (44, 44, 20, 20, 2, 2, 144, 56, 1),
    'sq48': (48, 48, 22, 22, 2, 2, 174, 68, 1),
    'sq52': (52, 52, 24, 24, 2, 2, 102, 42, 2),
    'sq64': (64, 64, 14, 14, 4, 4, 140, 56, 2),
    'sq72': (72, 72, 16, 16, 4, 4, 92, 36, 4),
    'sq80': (80, 80, 18, 18, 4, 4, 114, 48, 4),
    'sq88': (88, 88, 20, 20, 4, 4, 144, 56, 4),
    'sq96': (96, 96, 22, 22, 4, 4, 174, 68, 4),
    'sq104': (104, 104, 24, 24, 4, 4, 136, 56, 6),
    'sq120': (120, 120, 18, 18, 6, 6, 175, 68, 6),
    'sq132': (132, 132, 20, 20, 6, 6, 163, 62, 8),
    # there are two separate sections of the data matrix with different interleaving
    # and reed-solomon parameters. this will be handled separately.
    'sq144': (144, 144, 22, 22, 6, 6, 0, 0, 0),
    'rect8x18': (8, 18, 6, 16, 1, 1, 5, 7, 1),
    'rect8x32': (8, 32, 6, 14, 1, 2, 10, 11, 1),
    'rect12x26': (12, 26, 10, 24, 1, 1, 16, 14, 1),
    'rect12x36': (12, 36, 10, 16, 1, 2, 22, 18, 1),
    'rect16x36': (16, 36, 14, 16, 1, 2, 32, 24, 1),
    'rect16x48': (16, 48, 14, 22, 1, 2, 49, 28, 1),
}



# CODEWORD STREAM GENERATION =========================================
# take the text input and return the codewords,
# including the Reed-Solomon error-correcting codes.
# =====================================================================

def get_codewords(text, nd, nc, inter, size144):
    # convert the data to the codewords
    data = list(encode_to_ascii(text))

    if not size144:  # render a "normal" datamatrix
        data_blocks = partition_data(data, nd * inter)  # partition into data blocks of length nd*inter -> inter Reed-Solomon block

        data_blocks = interleave(data_blocks, inter)  # interleave consecutive inter blocks if required

        data_blocks = reed_solomon(data_blocks, nd, nc)  # generate and append the Reed-Solomon codewords

        data_blocks = combine_interleaved(data_blocks, inter, nd, nc, False)  # concatenate Reed-Solomon blocks bound for the same datamatrix

    else:  # we have a 144x144 datamatrix
        data_blocks = partition_data(data, 1558)  # partition the data into datamtrix-sized chunks (1558 =156*8 + 155*2 )

        for i in range(len(data_blocks)):  # for each datamtrix

            inter = 8
            nd = 156
            nc = 62
            block1 = data_blocks[i][0:156 * 8]
            block1 = interleave([block1], inter)  # interleave into 8 blocks
            block1 = reed_solomon(block1, nd, nc)  # generate and append the Reed-Solomon codewords

            inter = 2
            nd = 155
            nc = 62
            block2 = data_blocks[i][156 * 8:]
            block2 = interleave([block2], inter)  # interleave into 2 blocks
            block2 = reed_solomon(block2, nd, nc)  # generate and append the Reed-Solomon codewords

            blocks = block1
            blocks.extend(block2)

            blocks = combine_interleaved(blocks, 10, nd, nc, True)

            data_blocks[i] = blocks[0]

    return data_blocks


# Takes a codeword stream and splits up into "inter" blocks.
# eg interleave( [1,2,3,4,5,6], 2 ) -> [1,3,5], [2,4,6]
def interleave(blocks, inter):
    if inter == 1:  # if we don't have to interleave, just return the blocks
        return blocks
    else:
        result = []
        for block in blocks:  # for each codeword block in the stream
            block_length = int(len(block) / inter)  # length of each interleaved block
            inter_blocks = [[0] * block_length for i in range(inter)]  # the interleaved blocks

            for i in range(block_length):  # for each element in the interleaved blocks
                for j in range(inter):  # for each interleaved block
                    inter_blocks[j][i] = block[i * inter + j]

            result.extend(inter_blocks)  # add the interleaved blocks to the output

        return result


# Combine interleaved blocks into the groups for the same datamatrix
#
# e.g combine_interleaved( [[d1, d3, d5, e1, e3, e5], [d2, d4, d6, e2, e4, e6]], 2, 3, 3 )
#   --> [[d1, d2, d3, d4, d5, d6, e1, e2, e3, e4, e5, e6]]
def combine_interleaved(blocks, inter, nd, nc, size144):
    if inter == 1:  # the blocks aren't interleaved
        return blocks
    else:
        result = []
        for i in range(len(blocks) // inter):  # for each group of "inter" blocks -> one full datamatrix
            data_codewords = []  # interleaved data blocks

            if size144:
                nd_range = 1558  # 1558 = 156*8 + 155*2
                nc_range = 620  # 620 = 62*8 + 62*2
            else:
                nd_range = nd * inter
                nc_range = nc * inter

            for j in range(nd_range):  # for each codeword in the final list
                data_codewords.append(blocks[i * inter + j % inter][j // inter])

            for j in range(nc_range):  # for each block, add the ecc codewords
                data_codewords.append(blocks[i * inter + j % inter][nd + j // inter])

            result.append(data_codewords)
        return result


def encode_to_ascii(text):
    """Encode this text into chunks, ascii or digits"""
    i = 0
    while i < len(text):
        # check for double digits, if the next char is also a digit
        if text[i].isdigit() and (i < len(text) - 1) and text[i + 1].isdigit():
            yield int(text[i] + text[i + 1]) + 130
            i += 2  # move on 2 characters
        else:  # encode as a normal ascii,
            yield ord(text[i]) + 1  # codeword is ASCII value + 1 (ISO 16022:2006 5.2.3)
            i += 1  # next character


# partition data into blocks of the appropriate size to suit the
# Reed-Solomon block being used.
# e.g. partition_data([1,2,3,4,5], 3) -> [[1,2,3],[4,5,PAD]]
def partition_data(data, rs_data):
    PAD_VAL = 129  # PAD codeword (ISO 16022:2006 5.2.3)
    data_blocks = []
    i = 0
    while i < len(data):
        if len(data) >= i + rs_data:  # we have a whole block in our data
            data_blocks.append(data[i:i + rs_data])
            i = i + rs_data
        else:  # pad out with the pad codeword
            data_block = data[i:len(data)]  # add any remaining data
            pad_pos = len(data)
            padded = False
            while len(data_block) < rs_data:  # and then pad with randomised pad codewords
                if not padded:
                    data_block.append(PAD_VAL)  # add a normal pad codeword
                    padded = True
                else:
                    data_block.append(randomise_pad_253(PAD_VAL, pad_pos))
                pad_pos += 1
            data_blocks.append(data_block)
            break

    return data_blocks


# Pad character randomisation, to prevent regular patterns appearing
# in the data matrix
def randomise_pad_253(pad_value, pad_position):
    pseudo_random_number = ((149 * pad_position) % 253) + 1
    randomised = pad_value + pseudo_random_number
    if randomised <= 254:
        return randomised
    else:
        return randomised - 254


# REED-SOLOMON ENCODING ROUTINES =====================================

# "prod(x,y,log,alog,gf)" returns the product "x" times "y"
def prod(x, y, log, alog, gf):
    if x == 0 or y == 0:
        return 0
    else:
        result = alog[(log[x] + log[y]) % (gf - 1)]
        return result


# generate the log & antilog lists:
def gen_log_alog(gf, pp):
    log = [0] * gf
    alog = [0] * gf

    log[0] = 1 - gf
    alog[0] = 1

    for i in range(1, gf):
        alog[i] = alog[i - 1] * 2

        if alog[i] >= gf:
            alog[i] = alog[i] ^ pp

        log[alog[i]] = i

    return log, alog


# generate the generator polynomial coefficients:
def gen_poly_coeffs(nc, log, alog, gf):
    c = [0] * (nc + 1)
    c[0] = 1

    for i in range(1, nc + 1):
        c[i] = c[i - 1]

        j = i - 1
        while j >= 1:
            c[j] = c[j - 1] ^ prod(c[j], alog[i], log, alog, gf)
            j -= 1

        c[0] = prod(c[0], alog[i], log, alog, gf)

    return c


# "ReedSolomon(wd,nd,nc)" takes "nd" data codeword values in wd[]
# and adds on "nc" check codewords, all within GF(gf) where "gf" is a
# power of 2 and "pp" is the value of its prime modulus polynomial */
def reed_solomon(data, nd, nc):
    # parameters of the polynomial arithmetic
    gf = 256  # operating on 8-bit codewords -> Galois field = 2^8 = 256
    pp = 301  # prime modulus polynomial for ECC-200 is 0b100101101 = 301 (ISO 16022:2006 5.7.1)

    log, alog = gen_log_alog(gf, pp)
    c = gen_poly_coeffs(nc, log, alog, gf)

    for block in data:  # for each block of data codewords

        block.extend([0] * (nc + 1))  # extend to make space for the error codewords

        # generate "nc" checkwords in the list block
        for i in range(0, nd):
            k = block[nd] ^ block[i]

            for j in range(0, nc):
                block[nd + j] = block[nd + j + 1] ^ prod(k, c[nc - j - 1], log, alog, gf)

        block.pop()

    return data


# MODULE PLACEMENT ROUTINES===========================================
#   These routines take a steam of codewords, and place them into the
#   DataMatrix in accordance with Annex F of BS ISO/IEC 16022:2006

def bit(byte, bit_ch):
    """bit() returns the bit'th bit of the byte"""
    # the MSB is bit 1, LSB is bit 8
    return (byte >> (8 - bit_ch)) % 2


def module(array, nrow, ncol, row, col, bit_ch):
    """place a given bit with appropriate wrapping within array"""
    if row < 0:
        row = row + nrow
        col = col + 4 - ((nrow + 4) % 8)

    if col < 0:
        col = col + ncol
        row = row + 4 - ((ncol + 4) % 8)

    array[row][col] = bit_ch


def place_square(case, array, nrow, ncol, row, col, char):
    """Populate corner cases (0-3) and utah case (-1)"""
    for i in range(8):
        x, y = [
            [(row - 1, 0), (row - 1, 1), (row - 1, 2), (0, col - 2),
             (0, col - 1), (1, col - 1), (2, col - 1), (3, col - 1)],
            [(row - 3, 0), (row - 2, 0), (row - 1, 0), (0, col - 4),
             (0, col - 3), (0, col - 2), (0, col - 1), (1, col - 1)],
            [(row - 3, 0), (row - 2, 0), (row - 1, 0), (0, col - 2),
             (0, col - 1), (1, col - 1), (2, col - 1), (3, col - 1)],
            [(row - 1, 0), (row - 1, col - 1), (0, col - 3), (0, col - 2),
             (0, col - 1), (1, col - 3), (1, col - 2), (1, col - 1)],

            # "utah" places the 8 bits of a utah-shaped symbol character in ECC200
            [(row - 2, col -2), (row - 2, col -1), (row - 1, col - 2), (row - 1, col - 1),
             (row - 1, col), (row, col - 2), (row, col - 1), (row, col)],
        ][case][i]
        module(array, nrow, ncol, x, y, bit(char, i + 1))
    return 1

def place_bits(data, nrow, ncol):
    """fill an nrow x ncol array with the bits from the codewords in data."""
    # initialise and fill with -1's (invalid value)
    array = [[INVALID_BIT] * ncol for i in range(nrow)]
    # Starting in the correct location for character #1, bit 8,...
    char = 0
    row = 4
    col = 0
    while True:

        # first check for one of the special corner cases, then...
        if (row == nrow) and (col == 0):
            char += place_square(0, array, nrow, ncol, nrow, ncol, data[char])
        elif (row == nrow - 2) and (col == 0) and (ncol % 4):
            char += place_square(1, array, nrow, ncol, nrow, ncol, data[char])
        elif (row == nrow - 2) and (col == 0) and (ncol % 8 == 4):
            char += place_square(2, array, nrow, ncol, nrow, ncol, data[char])
        elif (row == nrow + 4) and (col == 2) and ((ncol % 8) == 0):
            char += place_square(3, array, nrow, ncol, nrow, ncol, data[char])

        # sweep upward diagonally, inserting successive characters,...
        while (row >= 0) and (col < ncol):
            if (row < nrow) and (col >= 0) and (array[row][col] == INVALID_BIT):
                char += place_square(-1, array, nrow, ncol, row, col, data[char])
            row -= 2
            col += 2

        row += 1
        col += 3

        # & then sweep downward diagonally, inserting successive characters,...
        while (row < nrow) and (col >= 0):
            if (row >= 0) and (col < ncol) and (array[row][col] == INVALID_BIT):
                char += place_square(-1, array, nrow, ncol, row, col, data[char])
            row += 2
            col -= 2

        row += 3
        col += 1

        # ... until the entire array is scanned
        if not ((row < nrow) or (col < ncol)):
            break

    # Lastly, if the lower righthand corner is untouched, fill in fixed pattern */
    if array[nrow - 1][ncol - 1] == INVALID_BIT:
        array[nrow - 1][ncol - 2] = 0
        array[nrow - 1][ncol - 1] = 1
        array[nrow - 2][ncol - 1] = 0
        array[nrow - 2][ncol - 2] = 1

    return array  # return the array of 1's and 0's


def add_finder_pattern(array, data_nrow, data_ncol, reg_row, reg_col):
    # get the total size of the datamatrix
    nrow = (data_nrow + 2) * reg_row
    ncol = (data_ncol + 2) * reg_col

    datamatrix = [[0] * ncol for i in range(nrow)]  # initialise and fill with 0's

    for i in range(reg_col):  # for each column of data regions
        for j in range(nrow):
            datamatrix[j][i * (data_ncol + 2)] = 1  # vertical black bar on left
            datamatrix[j][i * (data_ncol + 2) + data_ncol + 1] = j % 2  # alternating blocks

    for i in range(reg_row):  # for each row of data regions
        for j in range(ncol):
            datamatrix[i * (data_nrow + 2) + data_nrow + 1][j] = 1  # horizontal black bar at bottom
            datamatrix[i * (data_nrow + 2)][j] = (j + 1) % 2  # alternating blocks

    for i in range(data_nrow * reg_row):
        for j in range(data_ncol * reg_col):
            # offset by 1, plus two for every addition block
            dest_col = j + 1 + 2 * (j // data_ncol)
            dest_row = i + 1 + 2 * (i // data_nrow)

            datamatrix[dest_row][dest_col] = array[i][j]  # transfer from the plain bit array

    return datamatrix




class DataMatrix(inkex.GenerateExtension):
    container_label = 'DataMatrix'

    def add_arguments(self, pars):
        pars.add_argument("--text", default='Inkscape')
        pars.add_argument("--symbol", type=self.arg_symbols, required=True)
        pars.add_argument("--size", type=int, default=4)

    @staticmethod
    def arg_symbols(value):
        """Turn a symbol key into matrix metrics"""
        try:
            return SYMBOLS[value]
        except KeyError:
            raise inkex.AbortExtension("Invalid symbol size.")

    def generate(self):
        size = str(self.options.size)
        style = inkex.Style({'stroke': 'none', 'stroke-width': '1', 'fill': '#000000'})
        attribs = {'style': str(style), 'height': size, 'width': size}

        if not self.options.text:
            raise inkex.AbortExtension("Please enter an input string.")

        # create a 2d list corresponding to the 1's and 0s of the DataMatrix
        encoded = self.encode(self.options.text, *self.options.symbol)
        for x, y in self.render_data_matrix(encoded):
            attribs.update({'x': str(x), 'y': str(y)})
            yield Rectangle(**attribs)

    def encode(self, text, nrow, ncol, data_nrow, data_ncol, reg_row, reg_col, nd, nc, inter):
        """
        Take an input string and convert it to a sequence (or sequences)
        of codewords as specified in ISO/IEC 16022:2006 (section 5.2.3)
        """
        # generate the codewords including padding and ECC
        codewords = get_codewords(text, nd, nc, inter, nrow == 144)

        # break up into separate arrays if more than one DataMatrix is needed
        module_arrays = []
        for codeword_stream in codewords:  # for each datamatrix
            # place the codewords' bits across the array as modules
            bit_array = place_bits(codeword_stream, data_nrow * reg_row, data_ncol * reg_col)
            # add finder patterns around the modules
            module_arrays.append(add_finder_pattern(bit_array, data_nrow, data_ncol, reg_row, reg_col))

        return module_arrays

    def render_data_matrix(self, module_arrays):
        """turn a 2D array of 1's and 0's into a set of black squares"""
        ncol = self.options.symbol[1]
        size = self.options.size
        spacing = ncol * size * 1.5
        for i, line in enumerate(module_arrays):
            height = len(line)
            width = len(line[0])

            for y in range(height):  # loop over all the modules in the datamatrix
                for x in range(width):
                    if line[y][x] == 1:  # A binary 1 is a filled square
                        yield (x * size + i * spacing, y * size)
                    elif line[y][x] == INVALID_BIT:  # we have an invalid bit value
                        inkex.errormsg('Invalid bit value, {}!'.format(line[y][x]))

if __name__ == '__main__':
    DataMatrix().run()

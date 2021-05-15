#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2019 Marc Jeanmougin
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
# Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA
#
"""
Generate Latex via a PDF using pdflatex
"""

import os

import inkex
from inkex.base import TempDirMixin
from inkex.command import call, inkscape
from inkex import load_svg, ShapeElement, Defs

class PdfLatex(TempDirMixin, inkex.GenerateExtension):
    """
    Use pdflatex to generate LaTeX, this whole hack is required because
    we don't want to open a LaTeX document as a document, but as a
    generated fragment (like import, but done manually).
    """
    def add_arguments(self, pars):
        pars.add_argument('--formule', type=str, default='')
        pars.add_argument('--packages', type=str, default='')

    def generate(self):
        tex_file = os.path.join(self.tempdir, 'input.tex')
        pdf_file = os.path.join(self.tempdir, 'input.pdf') # Auto-generate by pdflatex
        svg_file = os.path.join(self.tempdir, 'output.svg')

        with open(tex_file, 'w') as fhl:
            self.write_latex(fhl)

        call('pdflatex', tex_file,\
            output_directory=self.tempdir,\
            halt_on_error=True, oldie=True)

        inkscape(pdf_file, export_filename=svg_file, pdf_page=1,
                 pdf_poppler=True, export_type="svg")

        with open(svg_file, 'r') as fhl:
            svg = load_svg(fhl).getroot()
            svg.set_random_ids(backlinks=True)
            for child in svg:
                if isinstance(child, ShapeElement):
                    yield child
                elif isinstance(child, Defs):
                    for def_child in child:
                        #def_child.set_random_id()
                        self.svg.defs.append(def_child)

    def write_latex(self, stream):
        """Takes a forumle and wraps it in latex"""
        stream.write(r"""%% processed with pdflatex.py
\documentclass{minimal}
\usepackage{amsmath}
\usepackage{amssymb}
\usepackage{amsfonts}
""")
        for package in self.options.packages.split(','):
            if package:
                stream.write('\\usepackage{{{}}}\n'.format(package))
        stream.write("\n\\begin{document}\n")
        stream.write(self.options.formule)
        stream.write("\n\\end{document}\n")

if __name__ == '__main__':
    PdfLatex().run()

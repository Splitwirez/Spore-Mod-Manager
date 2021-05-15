#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2007 Terry Brown, terry_n_brown@yahoo.com
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
from __future__ import absolute_import, unicode_literals

import inkex
from inkex import Use, Rectangle
from inkex.base import SvgOutputMixin

class Nup(inkex.OutputExtension, SvgOutputMixin):
    """N-up Layout generator"""
    def add_arguments(self, pars):
        pars.add_argument('--unit', default='px')
        pars.add_argument('--rows', type=int, default=2)
        pars.add_argument('--cols', type=int, default=2)
        pars.add_argument('--paddingTop', type=float)
        pars.add_argument('--paddingBottom', type=float)
        pars.add_argument('--paddingLeft', type=float)
        pars.add_argument('--paddingRight', type=float)
        pars.add_argument('--marginTop', type=float)
        pars.add_argument('--marginBottom', type=float)
        pars.add_argument('--marginLeft', type=float)
        pars.add_argument('--marginRight', type=float)
        pars.add_argument('--pgMarginTop', type=float)
        pars.add_argument('--pgMarginBottom', type=float)
        pars.add_argument('--pgMarginLeft', type=float)
        pars.add_argument('--pgMarginRight', type=float)
        pars.add_argument('--pgSizeX', type=float)
        pars.add_argument('--pgSizeY', type=float)
        pars.add_argument('--sizeX', type=float)
        pars.add_argument('--sizeY', type=float)
        pars.add_argument('--calculateSize', type=inkex.Boolean, default=True)
        pars.add_argument('--showHolder', type=inkex.Boolean, default=True)
        pars.add_argument('--showCrosses', type=inkex.Boolean, default=True)
        pars.add_argument('--showInner', type=inkex.Boolean, default=True)
        pars.add_argument('--showOuter', type=inkex.Boolean, default=False)
        pars.add_argument('--showInnerBox', type=inkex.Boolean, default=False)
        pars.add_argument('--showOuterBox', type=inkex.Boolean, default=False)
        pars.add_argument('--tab')

    def save(self, stream):
        show_list = []
        for i in ['showHolder', 'showCrosses', 'showInner', 'showOuter',
                  'showInnerBox', 'showOuterBox', ]:
            if getattr(self.options, i):
                show_list.append(i.lower().replace('show', ''))
        opt = self.options
        ret = self.generate_nup(
            unit=opt.unit,
            pgSize=(opt.pgSizeX, opt.pgSizeY),
            pgMargin=(opt.pgMarginTop, opt.pgMarginRight, opt.pgMarginBottom, opt.pgMarginLeft),
            num=(opt.rows, opt.cols),
            calculateSize=opt.calculateSize,
            size=(opt.sizeX, opt.sizeY),
            margin=(opt.marginTop, opt.marginRight, opt.marginBottom, opt.marginLeft),
            padding=(opt.paddingTop, opt.paddingRight, opt.paddingBottom, opt.paddingLeft),
            show=show_list,
        )
        if ret:
            stream.write(ret)

    def expandTuple(self, unit, x, length=4):
        try:
            iter(x)
        except:
            return None

        if len(x) != length:
            x *= 2
        if len(x) != length:
            raise Exception("expandTuple: requires 2 or 4 item tuple")
        try:
            return tuple(map(lambda ev: (self.svg.unittouu(str(eval(str(ev))) + unit) / self.svg.unittouu('1px')), x))
        except:
            return None

    def generate_nup(self,
                     unit="px",
                     pgSize=("8.5*96", "11*96"),
                     pgMargin=(0, 0),
                     pgPadding=(0, 0),
                     num=(2, 2),
                     calculateSize=True,
                     size=None,
                     margin=(0, 0),
                     padding=(20, 20),
                     show=['default'],
                    ):
        """Generate the SVG.  Inputs are run through 'eval(str(x))' so you can use
    '8.5*72' instead of 612.  Margin / padding dimension tuples can be
    (top & bottom, left & right) or (top, right, bottom, left).

    Keyword arguments:
    pgSize -- page size, width x height
    pgMargin -- extra space around each page
    pgPadding -- added to pgMargin
    n -- rows x cols
    size -- override calculated size, width x height
    margin -- white space around each piece
    padding -- inner padding for each piece
    show -- list of keywords indicating what to show
            - 'crosses' - cutting guides
            - 'inner' - inner boundary
            - 'outer' - outer boundary
    """

        if 'default' in show:
            show = set(show).union(['inner', 'innerbox', 'holder', 'crosses'])

        pgMargin = self.expandTuple(unit, pgMargin)
        pgPadding = self.expandTuple(unit, pgPadding)
        margin = self.expandTuple(unit, margin)
        padding = self.expandTuple(unit, padding)

        pgSize = self.expandTuple(unit, pgSize, length=2)
        #    num = tuple(map(lambda ev: eval(str(ev)), num))

        if not pgMargin or not pgPadding:
            return inkex.errormsg("No padding or margin available.")

        page_edge = list(map(sum, zip(pgMargin, pgPadding)))

        top, right, bottom, left = 0, 1, 2, 3
        width, height = 0, 1
        rows, cols = 0, 1
        size = self.expandTuple(unit, size, length=2)
        if size is None or calculateSize or len(size) < 2 or size[0] == 0 or size[1] == 0:
            size = ((pgSize[width]
                     - page_edge[left] - page_edge[right]
                     - num[cols] * (margin[left] + margin[right])) / num[cols],
                    (pgSize[height]
                     - page_edge[top] - page_edge[bottom]
                     - num[rows] * (margin[top] + margin[bottom])) / num[rows]
                   )
        else:
            size = self.expandTuple(unit, size, length=2)

        # sep is separation between same points on pieces
        sep = (size[width] + margin[right] + margin[left],
               size[height] + margin[top] + margin[bottom])

        style = 'stroke:#000000;stroke-opacity:1;fill:none;fill-opacity:1;'

        padbox = Rectangle(
            x=str(page_edge[left] + margin[left] + padding[left]),
            y=str(page_edge[top] + margin[top] + padding[top]),
            width=str(size[width] - padding[left] - padding[right]),
            height=str(size[height] - padding[top] - padding[bottom]),
            style=style,
        )
        margbox = Rectangle(
            x=str(page_edge[left] + margin[left]),
            y=str(page_edge[top] + margin[top]),
            width=str(size[width]),
            height=str(size[height]),
            style=style,
        )

        doc = self.get_template(width=pgSize[width], height=pgSize[height])
        svg = doc.getroot()

        def make_clones(under, to):
            for row in range(0, num[rows]):
                for col in range(0, num[cols]):
                    if row == 0 and col == 0:
                        continue
                    use = under.add(Use())
                    use.set('xlink:href', '#' + to)
                    use.transform.add_translate(col * sep[width], row * sep[height])

        # guidelayer #####################################################
        if {'inner', 'outer'}.intersection(show):
            layer = svg.add(inkex.Layer.new('Guide Layer'))
            if 'inner' in show:
                ibox = layer.add(padbox.copy())
                ibox.style['stroke'] = '#8080ff'
                ibox.set('id', 'innerguide')
                make_clones(layer, 'innerguide')
            if 'outer' in show:
                obox = layer.add(margbox.copy())
                obox.style['stroke'] = '#8080ff'
                obox.set('id', 'outerguide')
                make_clones(layer, 'outerguide')

        # crosslayer #####################################################
        if {'crosses'}.intersection(show):
            layer = svg.add(inkex.Layer.new('Cut Layer'))

            if 'crosses' in show:
                crosslen = 12
                group = layer.add(inkex.Group(id='cross'))
                x, y = 0, 0
                path = 'M%f %f' % (x + page_edge[left] + margin[left],
                                   y + page_edge[top] + margin[top] - crosslen)
                path += ' L%f %f' % (x + page_edge[left] + margin[left],
                                     y + page_edge[top] + margin[top] + crosslen)
                path += ' M%f %f' % (x + page_edge[left] + margin[left] - crosslen,
                                     y + page_edge[top] + margin[top])
                path += ' L%f %f' % (x + page_edge[left] + margin[left] + crosslen,
                                     y + page_edge[top] + margin[top])
                group.add(inkex.PathElement(style=style + 'stroke-width:0.05',
                                            d=path, id='crossmarker'))
                for row in 0, 1:
                    for col in 0, 1:
                        if row or col:
                            cln = group.add(Use())
                            cln.set('xlink:href', '#crossmarker')
                            cln.transform.add_translate(col * size[width], row * size[height])
                make_clones(layer, 'cross')

        # clonelayer #####################################################
        layer = svg.add(inkex.Layer.new('Clone Layer'))
        make_clones(layer, 'main')

        # mainlayer ######################################################
        layer = svg.add(inkex.Layer.new('Main Layer'))
        group = layer.add(inkex.Group(id='main'))

        if 'innerbox' in show:
            group.add(padbox)
        if 'outerbox' in show:
            group.add(margbox)
        if 'holder' in show:
            x, y = (page_edge[left] + margin[left] + padding[left],
                    page_edge[top] + margin[top] + padding[top])
            w, h = (size[width] - padding[left] - padding[right],
                    size[height] - padding[top] - padding[bottom])
            path = 'M{:f} {:f}'.format(x + w / 2., y)
            path += ' L{:f} {:f}'.format(x + w, y + h / 2.)
            path += ' L{:f} {:f}'.format(x + w / 2., y + h)
            path += ' L{:f} {:f}'.format(x, y + h / 2.)
            path += ' Z'
            group.add(inkex.PathElement(style=style, d=path))

        return svg.tostring()


if __name__ == '__main__':
    Nup().run()

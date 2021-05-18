#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2005 Aaron Spike, aaron@ekips.org
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

import random
import inkex
from inkex import turtle as pturtle

class Lindenmayer(inkex.GenerateExtension):
    def add_arguments(self, pars):
        pars.add_argument("--tab")
        pars.add_argument("--order", type=int, default=3, help="number of iteration")
        pars.add_argument("--langle", type=float, default=16.0, help="angle for turning left")
        pars.add_argument("--rangle", type=float, default=16.0, help="angle for turning right")
        pars.add_argument("--step", type=float, default=25.0, help="step size")
        pars.add_argument("--randomizestep", type=float, default=0.0, help="randomize step")
        pars.add_argument("--randomizeangle", type=float, default=0.0, help="randomize angle")
        pars.add_argument("--axiom", default="++F", help="initial state of system")
        pars.add_argument("--rules", default="F=FF-[-F+F+F]+[+F-F-F]", help="replacement rules")
        self.stack = []
        self.turtle = pturtle.pTurtle()

    def iterate(self):
        self.rules = dict([map((lambda s: s.strip()), i.split("=")) for i in self.options.rules.upper().split(";") if i.count("=") == 1])
        string = self.__recurse(self.options.axiom.upper(), 0)
        self.__compose_path(string)
        return self.turtle.getPath()

    def __compose_path(self, string):
        self.turtle.pu()
        point = self.svg.namedview.center
        self.turtle.setpos(point)
        self.turtle.pd()
        for c in string:
            if c in 'ABCDEF':
                self.turtle.pd()
                self.turtle.fd(self.options.step * (random.normalvariate(1.0, 0.01 * self.options.randomizestep)))
            elif c in 'GHIJKL':
                self.turtle.pu()
                self.turtle.fd(self.options.step * (random.normalvariate(1.0, 0.01 * self.options.randomizestep)))
            elif c == '+':
                self.turtle.lt(self.options.langle * (random.normalvariate(1.0, 0.01 * self.options.randomizeangle)))
            elif c == '-':
                self.turtle.rt(self.options.rangle * (random.normalvariate(1.0, 0.01 * self.options.randomizeangle)))
            elif c == '|':
                self.turtle.lt(180)
            elif c == '[':
                self.stack.append([self.turtle.getpos(), self.turtle.getheading()])
            elif c == ']':
                self.turtle.pu()
                pos, heading = self.stack.pop()
                self.turtle.setpos(pos)
                self.turtle.setheading(heading)

    def __recurse(self, rule, level):
        level_string = ''
        for c in rule:
            if level < self.options.order:
                try:
                    level_string = level_string + self.__recurse(self.rules[c], level + 1)
                except KeyError:
                    level_string = level_string + c
            else:
                level_string = level_string + c
        return level_string

    def generate(self):
        self.options.step = self.svg.unittouu(str(self.options.step) + 'px')
        sty = {'stroke-linejoin': 'miter', 'stroke-width': str(self.svg.unittouu('1px')),
             'stroke-opacity': '1.0', 'fill-opacity': '1.0',
             'stroke': '#000000', 'stroke-linecap': 'butt',
             'fill': 'none'}
        return inkex.PathElement(style=str(inkex.Style(sty)), d=self.iterate())


if __name__ == '__main__':
    Lindenmayer().run()

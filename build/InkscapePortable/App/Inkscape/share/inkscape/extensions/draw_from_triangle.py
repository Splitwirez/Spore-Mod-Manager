#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2007 John Beard john.j.beard@gmail.com
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
This extension allows you to draw various triangle constructions
It requires a path to be selected
It will use the first three nodes of this path

Dimensions of a triangle__

       /`__
      / a_c``--__
     /           ``--__ s_a
s_b /                  ``--__
   /a_a                    a_b`--__
  /--------------------------------``B
 A              s_b
"""

from math import acos, cos, pi, sin, sqrt, tan

import inkex
from inkex import PathElement, Circle

(X, Y) = range(2)

# DRAWING ROUTINES

# draw an SVG triangle given in trilinar coords
def draw_SVG_circle(rad, centre, params, style, name, parent):  # draw an SVG circle with a given radius as trilinear coordinates
    if rad == 0:  # we want a dot
        r = style.d_rad  # get the dot width from the style
        circ_style = {'stroke': style.d_col, 'stroke-width': str(style.d_th), 'fill': style.d_fill}
    else:
        r = rad  # use given value
        circ_style = {'stroke': style.c_col, 'stroke-width': str(style.c_th), 'fill': style.c_fill}

    cx, cy = get_cartesian_pt(centre, params)
    circ_attribs = {'cx': str(cx), 'cy': str(cy), 'r': str(r)}
    elem = parent.add(Circle(**circ_attribs))
    elem.style = circ_style
    elem.label = name


# draw an SVG triangle given in trilinar coords
def draw_SVG_tri(vert_mat, params, style, name, parent):
    p1, p2, p3 = get_cartesian_tri(vert_mat, params)  # get the vertex matrix in cartesian points
    elem = parent.add(PathElement())
    elem.path = 'M ' + str(p1[0]) + ',' + str(p1[1]) +\
                ' L ' + str(p2[0]) + ',' + str(p2[1]) +\
                ' L ' + str(p3[0]) + ',' + str(p3[1]) +\
                ' L ' + str(p1[0]) + ',' + str(p1[1]) + ' z'
    elem.style = {'stroke': style.l_col, 'stroke-width': str(style.l_th), 'fill': style.l_fill}
    elem.label = name


# draw an SVG line segment between the given (raw) points
def draw_SVG_line(a, b, style, name, parent):
    (x1, y1) = a
    (x2, y2) = b
    line = parent.add(PathElement())
    line.style = {'stroke': style.l_col, 'stroke-width': str(style.l_th), 'fill': style.l_fill}
    line.path = 'M ' + str(x1) + ',' + str(y1) + ' L ' + str(x2) + ',' + str(y2)
    line.lavel = name


# lines from each vertex to a corresponding point in trilinears
def draw_vertex_lines(vert_mat, params, width, name, parent):
    for i in range(3):
        oppositepoint = get_cartesian_pt(vert_mat[i], params)
        draw_SVG_line(params[3][-i % 3], oppositepoint, width, name + ':' + str(i), parent)


# MATHEMATICAL ROUTINES

def distance(a, b):
    """find the pythagorean distance"""
    (x0, y0) = a
    (x1, y1) = b
    return sqrt((x0 - x1) * (x0 - x1) + (y0 - y1) * (y0 - y1))


def vector_from_to(a, b):
    """get the vector from (x0,y0) to (x1,y1)"""
    return b[X] - a[X], b[Y], a[Y]


def get_cartesian_pt(t, p):  # get the cartesian coordinates from a trilinear set
    denom = p[0][0] * t[0] + p[0][1] * t[1] + p[0][2] * t[2]
    c1 = p[0][1] * t[1] / denom
    c2 = p[0][2] * t[2] / denom
    return c1 * p[2][1][0] + c2 * p[2][0][0], c1 * p[2][1][1] + c2 * p[2][0][1]


def get_cartesian_tri(arg, params):
    """get the cartesian points from a trilinear vertex matrix"""
    (t11, t12, t13), (t21, t22, t23), (t31, t32, t33) = arg
    p1 = get_cartesian_pt((t11, t12, t13), params)
    p2 = get_cartesian_pt((t21, t22, t23), params)
    p3 = get_cartesian_pt((t31, t32, t33), params)
    return p1, p2, p3


def angle_from_3_sides(a, b, c):  # return the angle opposite side c
    cosx = (a * a + b * b - c * c) / (2 * a * b)  # use the cosine rule
    return acos(cosx)


def translate_string(string, os):  # translates s_a, a_a, etc to params[x][y], with cyclic offset
    string = string.replace('s_a', 'params[0][' + str((os + 0) % 3) + ']')  # replace with ref. to the relvant values,
    string = string.replace('s_b', 'params[0][' + str((os + 1) % 3) + ']')  # cycled by i
    string = string.replace('s_c', 'params[0][' + str((os + 2) % 3) + ']')
    string = string.replace('a_a', 'params[1][' + str((os + 0) % 3) + ']')
    string = string.replace('a_b', 'params[1][' + str((os + 1) % 3) + ']')
    string = string.replace('a_c', 'params[1][' + str((os + 2) % 3) + ']')
    string = string.replace('area', 'params[4][0]')
    string = string.replace('semiperim', 'params[4][1]')
    return string


def pt_from_tcf(tcf, params):  # returns a trilinear triplet from a triangle centre function
    trilin_pts = []  # will hold the final points
    for i in range(3):
        temp = tcf  # read in the tcf
        temp = translate_string(temp, i)
        func = eval('lambda params: ' + temp.strip('"'))  # the function leading to the trilinar element
        trilin_pts.append(func(params))  # evaluate the function for the first trilinear element
    return trilin_pts


# SVG DATA PROCESSING

def get_n_points_from_path(node, n):
    """returns a list of first n points (x,y) in an SVG path-representing node"""
    points = list(node.path.control_points)
    if len(points) < 3:
        return []
    return points[:3]

# EXTRA MATHS FUNCTIONS
def sec(x):  # secant(x)
    if x == pi / 2 or x == -pi / 2 or x == 3 * pi / 2 or x == -3 * pi / 2:  # sec(x) is undefined
        return 100000000000
    else:
        return 1 / cos(x)


def csc(x):  # cosecant(x)
    if x == 0 or x == pi or x == 2 * pi or x == -2 * pi:  # csc(x) is undefined
        return 100000000000
    else:
        return 1 / sin(x)


def cot(x):  # cotangent(x)
    if x == 0 or x == pi or x == 2 * pi or x == -2 * pi:  # cot(x) is undefined
        return 100000000000
    else:
        return 1 / tan(x)


class Style(object):  # container for style information
    def __init__(self, svg):
        # dot markers
        self.d_rad = svg.unittouu('4px')  # dot marker radius
        self.d_th = svg.unittouu('2px')  # stroke width
        self.d_fill = '#aaaaaa'  # fill colour
        self.d_col = '#000000'  # stroke colour

        # lines
        self.l_th = svg.unittouu('2px')
        self.l_fill = 'none'
        self.l_col = '#000000'

        # circles
        self.c_th = svg.unittouu('2px')
        self.c_fill = 'none'
        self.c_col = '#000000'


class DrawFromTriangle(inkex.EffectExtension):
    def add_arguments(self, pars):
        pars.add_argument("--tab")
        # PRESET POINT OPTIONS
        pars.add_argument("--circumcircle", type=inkex.Boolean, default=False)
        pars.add_argument("--circumcentre", type=inkex.Boolean, default=False)
        pars.add_argument("--incircle", type=inkex.Boolean, default=False)
        pars.add_argument("--incentre", type=inkex.Boolean, default=False)
        pars.add_argument("--contact_tri", type=inkex.Boolean, default=False)
        pars.add_argument("--excircles", type=inkex.Boolean, default=False)
        pars.add_argument("--excentres", type=inkex.Boolean, default=False)
        pars.add_argument("--extouch_tri", type=inkex.Boolean, default=False)
        pars.add_argument("--excentral_tri", type=inkex.Boolean, default=False)
        pars.add_argument("--orthocentre", type=inkex.Boolean, default=False)
        pars.add_argument("--orthic_tri", type=inkex.Boolean, default=False)
        pars.add_argument("--altitudes", type=inkex.Boolean, default=False)
        pars.add_argument("--anglebisectors", type=inkex.Boolean, default=False)
        pars.add_argument("--centroid", type=inkex.Boolean, default=False)
        pars.add_argument("--ninepointcentre", type=inkex.Boolean, default=False)
        pars.add_argument("--ninepointcircle", type=inkex.Boolean, default=False)
        pars.add_argument("--symmedians", type=inkex.Boolean, default=False)
        pars.add_argument("--sym_point", type=inkex.Boolean, default=False)
        pars.add_argument("--sym_tri", type=inkex.Boolean, default=False)
        pars.add_argument("--gergonne_pt", type=inkex.Boolean, default=False)
        pars.add_argument("--nagel_pt", type=inkex.Boolean, default=False)
        # CUSTOM POINT OPTIONS
        pars.add_argument("--mode", default='trilin')
        pars.add_argument("--cust_str", default='s_a')
        pars.add_argument("--cust_pt", type=inkex.Boolean, default=False)
        pars.add_argument("--cust_radius", type=inkex.Boolean, default=False)
        pars.add_argument("--radius", default='s_a')
        pars.add_argument("--isogonal_conj", type=inkex.Boolean, default=False)
        pars.add_argument("--isotomic_conj", type=inkex.Boolean, default=False)

    def effect(self):
        so = self.options  # shorthand

        pts = []  # initialise in case nothing is selected and following loop is not executed
        for node in self.svg.selection.filter(inkex.PathElement).values():
            # find the (x,y) coordinates of the first 3 points of the path
            pts = get_n_points_from_path(node, 3)

        if len(pts) == 3:  # if we have right number of nodes, else skip and end program
            st = Style(self.svg)  # style for dots, lines and circles

            # CREATE A GROUP TO HOLD ALL GENERATED ELEMENTS IN
            # Hold relative to point A (pt[0])
            layer = self.svg.get_current_layer().add(inkex.Group.new('TriangleElements'))
            layer.transform = 'translate(' + str(pts[0][0]) + ',' + str(pts[0][1]) + ')'

            # GET METRICS OF THE TRIANGLE
            # vertices in the local coordinates (set pt[0] to be the origin)
            vtx = [[0, 0],
                   [pts[1][0] - pts[0][0], pts[1][1] - pts[0][1]],
                   [pts[2][0] - pts[0][0], pts[2][1] - pts[0][1]]]

            s_a = distance(vtx[1], vtx[2])  # get the scalar side lengths
            s_b = distance(vtx[0], vtx[1])
            s_c = distance(vtx[0], vtx[2])
            sides = (s_a, s_b, s_c)  # side list for passing to functions easily and for indexing

            a_a = angle_from_3_sides(s_b, s_c, s_a)  # angles in radians
            a_b = angle_from_3_sides(s_a, s_c, s_b)
            a_c = angle_from_3_sides(s_a, s_b, s_c)
            angles = (a_a, a_b, a_c)

            ab = vector_from_to(vtx[0], vtx[1])  # vector from a to b
            ac = vector_from_to(vtx[0], vtx[2])  # vector from a to c
            bc = vector_from_to(vtx[1], vtx[2])  # vector from b to c
            vecs = (ab, ac)  # vectors for finding cartesian point from trilinears

            semiperim = (s_a + s_b + s_c) / 2.0  # semiperimeter
            area = sqrt(semiperim * (semiperim - s_a) * (semiperim - s_b) * (semiperim - s_c))  # area of the triangle by heron's formula
            uvals = (area, semiperim)  # useful values

            params = (sides, angles, vecs, vtx, uvals)  # all useful triangle parameters in one object

            # BEGIN DRAWING
            if so.circumcentre or so.circumcircle:
                r = s_a * s_b * s_c / (4 * area)
                pt = (cos(a_a), cos(a_b), cos(a_c))
                if so.circumcentre:
                    draw_SVG_circle(0, pt, params, st, 'Circumcentre', layer)
                if so.circumcircle:
                    draw_SVG_circle(r, pt, params, st, 'Circumcircle', layer)

            if so.incentre or so.incircle:
                pt = [1, 1, 1]
                if so.incentre:
                    draw_SVG_circle(0, pt, params, st, 'Incentre', layer)
                if so.incircle:
                    r = area / semiperim
                    draw_SVG_circle(r, pt, params, st, 'Incircle', layer)

            if so.contact_tri:
                t1 = s_b * s_c / (-s_a + s_b + s_c)
                t2 = s_a * s_c / (s_a - s_b + s_c)
                t3 = s_a * s_b / (s_a + s_b - s_c)
                v_mat = ((0, t2, t3), (t1, 0, t3), (t1, t2, 0))
                draw_SVG_tri(v_mat, params, st, 'ContactTriangle', layer)

            if so.extouch_tri:
                t1 = (-s_a + s_b + s_c) / s_a
                t2 = (s_a - s_b + s_c) / s_b
                t3 = (s_a + s_b - s_c) / s_c
                v_mat = ((0, t2, t3), (t1, 0, t3), (t1, t2, 0))
                draw_SVG_tri(v_mat, params, st, 'ExtouchTriangle', layer)

            if so.orthocentre:
                pt = pt_from_tcf('cos(a_b)*cos(a_c)', params)
                draw_SVG_circle(0, pt, params, st, 'Orthocentre', layer)

            if so.orthic_tri:
                v_mat = [[0, sec(a_b), sec(a_c)], [sec(a_a), 0, sec(a_c)], [sec(a_a), sec(a_b), 0]]
                draw_SVG_tri(v_mat, params, st, 'OrthicTriangle', layer)

            if so.centroid:
                pt = [1 / s_a, 1 / s_b, 1 / s_c]
                draw_SVG_circle(0, pt, params, st, 'Centroid', layer)

            if so.ninepointcentre or so.ninepointcircle:
                pt = [cos(a_b - a_c), cos(a_c - a_a), cos(a_a - a_b)]
                if so.ninepointcentre:
                    draw_SVG_circle(0, pt, params, st, 'NinePointCentre', layer)
                if so.ninepointcircle:
                    r = s_a * s_b * s_c / (8 * area)
                    draw_SVG_circle(r, pt, params, st, 'NinePointCircle', layer)

            if so.altitudes:
                v_mat = [[0, sec(a_b), sec(a_c)], [sec(a_a), 0, sec(a_c)], [sec(a_a), sec(a_b), 0]]
                draw_vertex_lines(v_mat, params, st, 'Altitude', layer)

            if so.anglebisectors:
                v_mat = ((0, 1, 1), (1, 0, 1), (1, 1, 0))
                draw_vertex_lines(v_mat, params, st, 'AngleBisectors', layer)

            if so.excircles or so.excentres or so.excentral_tri:
                v_mat = ((-1, 1, 1), (1, -1, 1), (1, 1, -1))
                if so.excentral_tri:
                    draw_SVG_tri(v_mat, params, st, 'ExcentralTriangle', layer)
                for i in range(3):
                    if so.excircles:
                        r = area / (semiperim - sides[i])
                        draw_SVG_circle(r, v_mat[i], params, st, 'Excircle:' + str(i), layer)
                    if so.excentres:
                        draw_SVG_circle(0, v_mat[i], params, st, 'Excentre:' + str(i), layer)

            if so.sym_tri or so.symmedians:
                v_mat = ((0, s_b, s_c), (s_a, 0, s_c), (s_a, s_b, 0))
                if so.sym_tri:
                    draw_SVG_tri(v_mat, params, st, 'SymmedialTriangle', layer)
                if so.symmedians:
                    draw_vertex_lines(v_mat, params, st, 'Symmedian', layer)

            if so.sym_point:
                pt = (s_a, s_b, s_c)
                draw_SVG_circle(0, pt, params, st, 'SymmmedianPoint', layer)

            if so.gergonne_pt:
                pt = pt_from_tcf('1/(s_a*(s_b+s_c-s_a))', params)
                draw_SVG_circle(0, pt, params, st, 'GergonnePoint', layer)

            if so.nagel_pt:
                pt = pt_from_tcf('(s_b+s_c-s_a)/s_a', params)
                draw_SVG_circle(0, pt, params, st, 'NagelPoint', layer)

            if so.cust_pt or so.cust_radius or so.isogonal_conj or so.isotomic_conj:
                pt = []  # where we will store the point in trilinears
                if so.mode == 'trilin':  # if we are receiving from trilinears
                    for i in range(3):
                        strings = so.cust_str.split(':')  # get split string
                        strings[i] = translate_string(strings[i], 0)
                        func = eval('lambda params: ' + strings[i].strip('"'))  # the function leading to the trilinar element
                        pt.append(func(params))  # evaluate the function for the trilinear element
                else:  # we need a triangle function
                    string = so.cust_str  # don't need to translate, as the pt_from_tcf function does that for us
                    pt = pt_from_tcf(string, params)  # get the point from the tcf directly

                if so.cust_pt:  # draw the point
                    draw_SVG_circle(0, pt, params, st, 'CustomTrilinearPoint', layer)
                if so.cust_radius:  # draw the circle with given radius
                    strings = translate_string(so.radius, 0)
                    func = eval('lambda params: ' + strings.strip('"'))  # the function leading to the radius
                    r = func(params)
                    draw_SVG_circle(r, pt, params, st, 'CustomTrilinearCircle', layer)
                if so.isogonal_conj:
                    isogonal = [0, 0, 0]
                    for i in range(3):
                        isogonal[i] = 1 / pt[i]
                    draw_SVG_circle(0, isogonal, params, st, 'CustomIsogonalConjugate', layer)
                if so.isotomic_conj:
                    isotomic = [0, 0, 0]
                    for i in range(3):
                        isotomic[i] = 1 / (params[0][i] * params[0][i] * pt[i])
                    draw_SVG_circle(0, isotomic, params, st, 'CustomIsotomicConjugate', layer)


if __name__ == '__main__':
    DrawFromTriangle().run()

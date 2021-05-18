#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2011 Vincent Nivoliers and contributors
#
# Contributors
#    ~suv, <suv-sf@users.sf.net>
# - Voronoi Diagram algorithm and C code by Steven Fortune, 1987, http://ect.bell-labs.com/who/sjf/
# - Python translation to file voronoi.py by Bill Simons, 2005, http://www.oxfish.com/
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
# Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
#
"""
Create Voronoi diagram from seeds (midpoints of selected objects)
"""

import random

import inkex
from inkex import Group, Rectangle, PathElement, Vector2d as Point

import voronoi

class Voronoi(inkex.EffectExtension):
    """Extension to create a Voronoi diagram."""
    def add_arguments(self, pars):
        pars.add_argument('--tab')
        pars.add_argument(
            '--diagram-type',
            default='Voronoi', dest='diagramType',
            choices=['Voronoi', 'Delaunay', 'Both'],
            help='Defines the type of the diagram')
        pars.add_argument(
            '--clip-box', choices=['Page', 'Automatic from seeds'],
            default='Page', dest='clip_box',
            help='Defines the bounding box of the Voronoi diagram')
        pars.add_argument(
            '--show-clip-box', type=inkex.Boolean,
            default=False, dest='showClipBox',
            help='Set this to true to write the bounding box')
        pars.add_argument(
            '--delaunay-fill-options', default="delaunay-no-fill",
            dest='delaunayFillOptions',
            help='Set the Delaunay triangles color options')

    def dot(self, x, y):
        """Clipping a line by a bounding box"""
        return x[0] * y[0] + x[1] * y[1]

    def intersect_line_segment(self, line, vt1, vt2):
        """Get the line intersection of the two verticies"""
        sc1 = self.dot(line, vt1) - line[2]
        sc2 = self.dot(line, vt2) - line[2]
        if sc1 * sc2 > 0:
            return 0, 0, False

        tmp = self.dot(line, vt1) - self.dot(line, vt2)
        if tmp == 0:
            return 0, 0, False
        und = (line[2] - self.dot(line, vt2)) / tmp
        vt0 = 1 - und
        return und * vt1[0] + vt0 * vt2[0], \
               und * vt1[1] + vt0 * vt2[1], \
               True

    def clip_edge(self, vertices, lines, edge, bbox):
        # bounding box corners
        bbc = [
            (bbox[0], bbox[2]),
            (bbox[1], bbox[2]),
            (bbox[1], bbox[3]),
            (bbox[0], bbox[3]),
        ]

        # record intersections of the line with bounding box edges
        if edge[0] >= len(lines):
            return []
        line = (lines[edge[0]])
        interpoints = []
        for i in range(4):
            pnt = self.intersect_line_segment(line, bbc[i], bbc[(i + 1) % 4])
            if pnt[2]:
                interpoints.append(pnt)

        # if the edge has no intersection, return empty intersection
        if len(interpoints) < 2:
            return []

        if len(interpoints) > 2:  # happens when the edge crosses the corner of the box
            interpoints = list(set(interpoints))  # remove doubles

        # points of the edge
        vt1 = vertices[edge[1]]
        interpoints.append((vt1[0], vt1[1], False))
        vt2 = vertices[edge[2]]
        interpoints.append((vt2[0], vt2[1], False))

        # sorting the points in the widest range to get them in order on the line
        minx = interpoints[0][0]
        miny = interpoints[0][1]
        maxx = interpoints[0][0]
        maxy = interpoints[0][1]
        for point in interpoints:
            minx = min(point[0], minx)
            maxx = max(point[0], maxx)
            miny = min(point[1], miny)
            maxy = max(point[1], maxy)

        if (maxx - minx) > (maxy - miny):
            interpoints.sort()
        else:
            interpoints.sort(key=lambda pt: pt[1])

        start = []
        inside = False  # true when the part of the line studied is in the clip box
        start_write = False  # true when the part of the line is in the edge segment
        for point in interpoints:
            if point[2]:  # The point is a bounding box intersection
                if inside:
                    if start_write:
                        return [[start[0], start[1]], [point[0], point[1]]]
                    return []
                else:
                    if start_write:
                        start = point
                inside = not inside
            else:  # The point is a segment endpoint
                if start_write:
                    if inside:
                        # a vertex ends the line inside the bounding box
                        return [[start[0], start[1]], [point[0], point[1]]]
                    return []
                else:
                    if inside:
                        start = point
                start_write = not start_write

    def effect(self):
        # Check that elements have been selected
        if not self.svg.selected:
            inkex.errormsg(_("Please select objects!"))
            return

        linestyle = {
            'stroke': '#000000',
            'stroke-width': str(self.svg.unittouu('1px')),
            'fill': 'none',
            'stroke-linecap': 'round',
            'stroke-linejoin': 'round'
        }

        facestyle = {
            'stroke': '#000000',
            'stroke-width': str(self.svg.unittouu('1px')),
            'fill': 'none',
            'stroke-linecap': 'round',
            'stroke-linejoin': 'round'
        }

        parent_group = self.svg.selection.first().getparent()
        trans = parent_group.composed_transform()

        invtrans = None
        if trans:
            invtrans = -trans

        # Recovery of the selected objects
        pts = []
        nodes = []
        seeds = []
        fills = []

        for node in self.svg.selected.values():
            nodes.append(node)
            bbox = node.bounding_box()
            if bbox:
                center_x, center_y = bbox.center
                point = [center_x, center_y]
                if trans:
                    point = trans.apply_to_point(point)
                pts.append(Point(*point))
                if self.options.delaunayFillOptions != "delaunay-no-fill":
                    fills.append(node.style.get('fill', 'none'))
                seeds.append(Point(center_x, center_y))

        # Creation of groups to store the result
        if self.options.diagramType != 'Delaunay':
            # Voronoi
            group_voronoi = parent_group.add(Group())
            group_voronoi.set('inkscape:label', 'Voronoi')
            if invtrans:
                group_voronoi.transform *= invtrans
        if self.options.diagramType != 'Voronoi':
            # Delaunay
            group_delaunay = parent_group.add(Group())
            group_delaunay.set('inkscape:label', 'Delaunay')

        # Clipping box handling
        if self.options.diagramType != 'Delaunay':
            # Clipping bounding box creation
            group_bbox = sum([node.bounding_box() for node in nodes], None)

            # Clipbox is the box to which the Voronoi diagram is restricted
            if self.options.clip_box == 'Page':
                svg = self.document.getroot()
                width = self.svg.unittouu(svg.get('width'))
                height = self.svg.unittouu(svg.get('height'))
                clip_box = (0, width, 0, height)
            else:
                clip_box = (2 * group_bbox[0] - group_bbox[1],
                            2 * group_bbox[1] - group_bbox[0],
                            2 * group_bbox[2] - group_bbox[3],
                            2 * group_bbox[3] - group_bbox[2])

            # Safebox adds points so that no Voronoi edge in clip_box is infinite
            safe_box = (2 * clip_box[0] - clip_box[1],
                        2 * clip_box[1] - clip_box[0],
                        2 * clip_box[2] - clip_box[3],
                        2 * clip_box[3] - clip_box[2])
            pts.append(Point(safe_box[0], safe_box[2]))
            pts.append(Point(safe_box[1], safe_box[2]))
            pts.append(Point(safe_box[1], safe_box[3]))
            pts.append(Point(safe_box[0], safe_box[3]))

            if self.options.showClipBox:
                # Add the clip box to the drawing
                rect = group_voronoi.add(Rectangle())
                rect.set('x', str(clip_box[0]))
                rect.set('y', str(clip_box[2]))
                rect.set('width', str(clip_box[1] - clip_box[0]))
                rect.set('height', str(clip_box[3] - clip_box[2]))
                rect.style = linestyle

        # Voronoi diagram generation
        if self.options.diagramType != 'Delaunay':
            vertices, lines, edges = voronoi.computeVoronoiDiagram(pts)
            for edge in edges:
                vindex1, vindex2 = edge[1:]
                if (vindex1 < 0) or (vindex2 < 0):
                    continue  # infinite lines have no need to be handled in the clipped box
                else:
                    segment = self.clip_edge(vertices, lines, edge, clip_box)
                    # segment = [vertices[vindex1],vertices[vindex2]] # deactivate clipping
                    if len(segment) > 1:
                        x1, y1 = segment[0]
                        x2, y2 = segment[1]
                        cmds = [['M', [x1, y1]], ['L', [x2, y2]]]
                        path = group_voronoi.add(PathElement())
                        path.set('d', str(inkex.Path(cmds)))
                        path.style = linestyle

        if self.options.diagramType != 'Voronoi':
            triangles = voronoi.computeDelaunayTriangulation(seeds)
            i = 0
            if self.options.delaunayFillOptions == "delaunay-fill":
                random.seed("inkscape")
            for triangle in triangles:
                pt1 = seeds[triangle[0]]
                pt2 = seeds[triangle[1]]
                pt3 = seeds[triangle[2]]
                cmds = [['M', [pt1.x, pt1.y]],
                        ['L', [pt2.x, pt2.y]],
                        ['L', [pt3.x, pt3.y]],
                        ['Z', []]]
                if self.options.delaunayFillOptions == "delaunay-fill" \
                    or self.options.delaunayFillOptions == "delaunay-fill-random":
                    facestyle = {
                        'stroke': fills[triangle[random.randrange(0, 2)]],
                        'stroke-width': str(self.svg.unittouu('0.005px')),
                        'fill': fills[triangle[random.randrange(0, 2)]],
                        'stroke-linecap': 'round',
                        'stroke-linejoin': 'round'
                    }
                path = group_delaunay.add(PathElement())
                path.set('d', str(inkex.Path(cmds)))
                path.style = facestyle
                i += 1

if __name__ == "__main__":
    Voronoi().run()

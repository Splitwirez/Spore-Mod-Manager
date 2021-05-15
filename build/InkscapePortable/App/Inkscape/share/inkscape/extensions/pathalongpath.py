#!/usr/bin/env python
#
# Copyright (C) 2006 Jean-Francois Barraud, barraud@math.univ-lille1.fr
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
# barraud@math.univ-lille1.fr
#
"""
This script deforms an object (the pattern) along other paths (skeletons)...
The first selected object is the pattern
the last selected ones are the skeletons.

Imagine a straight horizontal line L in the middle of the bounding box of the pattern.
Consider the normal bundle of L: the collection of all the vertical lines meeting L.
Consider this as the initial state of the plane; in particular, think of the pattern
as painted on these lines.

Now move and bend L to make it fit a skeleton, and see what happens to the normals:
they move and rotate, deforming the pattern.
"""
import copy

import inkex
from inkex.bezier import pointdistance, beziersplitatt, tpoint
from inkex.paths import CubicSuperPath

import pathmodifier

def flipxy(path):
    for pathcomp in path:
        for ctl in pathcomp:
            for pt in ctl:
                tmp = pt[0]
                pt[0] = -pt[1]
                pt[1] = -tmp


def offset(pathcomp, dx, dy):
    for ctl in pathcomp:
        for pt in ctl:
            pt[0] += dx
            pt[1] += dy


def stretch(pathcomp, xscale, yscale, org):
    for ctl in pathcomp:
        for pt in ctl:
            pt[0] = org[0] + (pt[0] - org[0]) * xscale
            pt[1] = org[1] + (pt[1] - org[1]) * yscale


def linearize(p, tolerance=0.001):
    """
    This function receives a component of a 'cubicsuperpath' and returns two things:
    The path subdivided in many straight segments, and an array containing the length of each segment.

    We could work with bezier path as well, but bezier arc lengths are (re)computed for each point
    in the deformed object. For complex paths, this might take a while.
    """
    zero = 0.000001
    i = 0
    d = 0
    lengths = []
    while i < len(p) - 1:
        box = pointdistance(p[i][1], p[i][2])
        box += pointdistance(p[i][2], p[i + 1][0])
        box += pointdistance(p[i + 1][0], p[i + 1][1])
        chord = pointdistance(p[i][1], p[i + 1][1])
        if (box - chord) > tolerance:
            b1, b2 = beziersplitatt([p[i][1], p[i][2], p[i + 1][0], p[i + 1][1]], 0.5)
            p[i][2][0], p[i][2][1] = b1[1]
            p[i + 1][0][0], p[i + 1][0][1] = b2[2]
            p.insert(i + 1, [[b1[2][0], b1[2][1]], [b1[3][0], b1[3][1]], [b2[1][0], b2[1][1]]])
        else:
            d = (box + chord) / 2
            lengths.append(d)
            i += 1
    new = [p[i][1] for i in range(0, len(p) - 1) if lengths[i] > zero]
    new.append(p[-1][1])
    lengths = [l for l in lengths if l > zero]
    return new, lengths


class PathAlongPath(pathmodifier.PathModifier):
    """Deform a path along a second path"""
    def add_arguments(self, pars):
        pars.add_argument("-n", "--noffset", type=float, default=0.0, help="normal offset")
        pars.add_argument("-t", "--toffset", type=float, default=0.0, help="tangential offset")
        pars.add_argument("-k", "--kind", type=str, default='')
        pars.add_argument("-c", "--copymode", default="Single",
                          help="repeat the path to fit deformer's length")
        pars.add_argument("-p", "--space", type=float, default=0.0)
        pars.add_argument("-v", "--vertical", type=inkex.Boolean, default=False,
                          help="reference path is vertical")
        pars.add_argument("-d", "--duplicate", type=inkex.Boolean, default=False,
                          help="duplicate pattern before deformation")
        pars.add_argument("--tab", help="The selected UI-tab when OK was pressed")

    def prepare_selection(self):
        """
        first selected->pattern, all but first selected-> skeletons
        """
        skeletons = self.svg.selection.paint_order()

        elem = skeletons.pop()
        if self.options.duplicate:
            elem = elem.duplicate()
        pattern = elem.to_path_element()
        elem.replace_with(pattern)

        self.expand_clones(skeletons, True, False)
        self.objects_to_paths(skeletons)
        return pattern, skeletons.id_dict()

    def lengthtotime(self, l):
        """
        Receives an arc length l, and returns the index of the segment in self.skelcomp
        containing the corresponding point, to gether with the position of the point on this segment.

        If the deformer is closed, do computations modulo the toal length.
        """
        if self.skelcompIsClosed:
            l = l % sum(self.lengths)
        if l <= 0:
            return 0, l / self.lengths[0]
        i = 0
        while (i < len(self.lengths)) and (self.lengths[i] <= l):
            l -= self.lengths[i]
            i += 1
        t = l / self.lengths[min(i, len(self.lengths) - 1)]
        return i, t

    def apply_diffeomorphism(self, bpt, vects=()):
        """
        The kernel of this stuff:
        bpt is a base point and for v in vectors, v'=v-p is a tangent vector at bpt.
        """
        s = bpt[0] - self.skelcomp[0][0]
        i, t = self.lengthtotime(s)
        if i == len(self.skelcomp) - 1:
            x, y = tpoint(self.skelcomp[i - 1], self.skelcomp[i], 1 + t)
            dx = (self.skelcomp[i][0] - self.skelcomp[i - 1][0]) / self.lengths[-1]
            dy = (self.skelcomp[i][1] - self.skelcomp[i - 1][1]) / self.lengths[-1]
        else:
            x, y = tpoint(self.skelcomp[i], self.skelcomp[i + 1], t)
            dx = (self.skelcomp[i + 1][0] - self.skelcomp[i][0]) / self.lengths[i]
            dy = (self.skelcomp[i + 1][1] - self.skelcomp[i][1]) / self.lengths[i]

        vx = 0
        vy = bpt[1] - self.skelcomp[0][1]
        if self.options.wave:
            bpt[0] = x + vx * dx
            bpt[1] = y + vy + vx * dy
        else:
            bpt[0] = x + vx * dx - vy * dy
            bpt[1] = y + vx * dy + vy * dx

        for v in vects:
            vx = v[0] - self.skelcomp[0][0] - s
            vy = v[1] - self.skelcomp[0][1]
            if self.options.wave:
                v[0] = x + vx * dx
                v[1] = y + vy + vx * dy
            else:
                v[0] = x + vx * dx - vy * dy
                v[1] = y + vx * dy + vy * dx

    def effect(self):
        if len(self.options.ids) < 2:
            raise inkex.AbortExtension("This extension requires two selected paths.")

        self.options.wave = (self.options.kind == "Ribbon")
        if self.options.copymode == "Single":
            self.options.repeat = False
            self.options.stretch = False
        elif self.options.copymode == "Repeated":
            self.options.repeat = True
            self.options.stretch = False
        elif self.options.copymode == "Single, stretched":
            self.options.repeat = False
            self.options.stretch = True
        elif self.options.copymode == "Repeated, stretched":
            self.options.repeat = True
            self.options.stretch = True

        pattern, skels = self.prepare_selection()
        bbox = pattern.bounding_box()

        if self.options.vertical:
            # flipxy(bbox)...
            bbox = inkex.BoundingBox(-bbox.y, -bbox.x)

        width = bbox.width
        delta_x = width + self.options.space
        if delta_x < 0.01:
            raise inkex.AbortExtension("The total length of the pattern is too small\n"\
                "Please choose a larger object or set 'Space between copies' > 0")

        if isinstance(pattern, inkex.PathElement):
            pattern.path = self._sekl_call(skels, pattern.path.to_superpath(), delta_x, bbox)

    def _sekl_call(self, skeletons, p0, dx, bbox):
        if self.options.vertical:
            flipxy(p0)
        newp = []
        for skelnode in skeletons.values():
            self.curSekeleton = skelnode.path.to_superpath()
            if self.options.vertical:
                flipxy(self.curSekeleton)
            for comp in self.curSekeleton:
                path = copy.deepcopy(p0)
                self.skelcomp, self.lengths = linearize(comp)
                # !!!!>----> TODO: really test if path is closed! end point==start point is not enough!
                self.skelcompIsClosed = (self.skelcomp[0] == self.skelcomp[-1])

                length = sum(self.lengths)
                xoffset = self.skelcomp[0][0] - bbox.x.minimum + self.options.toffset
                yoffset = self.skelcomp[0][1] - bbox.y.center - self.options.noffset

                if self.options.repeat:
                    NbCopies = max(1, int(round((length + self.options.space) / dx)))
                    width = dx * NbCopies
                    if not self.skelcompIsClosed:
                        width -= self.options.space
                    bbox.x.maximum = bbox.x.minimum + width
                    new = []
                    for sub in path:
                        for _ in range(NbCopies):
                            new.append(copy.deepcopy(sub))
                            offset(sub, dx, 0)
                    path = new

                for sub in path:
                    offset(sub, xoffset, yoffset)

                if self.options.stretch:
                    if not bbox.width:
                        raise inkex.AbortExtension("The 'stretch' option requires that the pattern must have non-zero width :\nPlease edit the pattern width.")
                    for sub in path:
                        stretch(sub, length / bbox.width, 1, self.skelcomp[0])

                for sub in path:
                    for ctlpt in sub:
                        self.apply_diffeomorphism(ctlpt[1], (ctlpt[0], ctlpt[2]))

                if self.options.vertical:
                    flipxy(path)
                newp += path
        return CubicSuperPath(newp)


if __name__ == '__main__':
    PathAlongPath().run()

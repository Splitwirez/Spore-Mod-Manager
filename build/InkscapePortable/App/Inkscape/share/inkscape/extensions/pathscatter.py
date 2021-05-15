#!/usr/bin/env python
# coding=utf-8
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
import random

import inkex
from inkex import bezier, Transform, BoundingBox, Group, Use

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
        box = bezier.pointdistance(p[i][1], p[i][2])
        box += bezier.pointdistance(p[i][2], p[i + 1][0])
        box += bezier.pointdistance(p[i + 1][0], p[i + 1][1])
        chord = bezier.pointdistance(p[i][1], p[i + 1][1])
        if (box - chord) > tolerance:
            b1, b2 = bezier.beziersplitatt([p[i][1], p[i][2], p[i + 1][0], p[i + 1][1]], 0.5)
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


class PathScatter(pathmodifier.Diffeo):
    def __init__(self):
        super(PathScatter, self).__init__()
        self.arg_parser.add_argument("-n", "--noffset", type=float, dest="noffset", default=0.0, help="normal offset")
        self.arg_parser.add_argument("-t", "--toffset", type=float, dest="toffset", default=0.0, help="tangential offset")
        self.arg_parser.add_argument("-g", "--grouppick", type=inkex.Boolean, dest="grouppick", default=False,
                                     help="if pattern is a group then randomly pick group members")
        self.arg_parser.add_argument("-m", "--pickmode", type=str, dest="pickmode", default="rand",
                                     help="group pick mode (rand=random seq=sequentially)")
        self.arg_parser.add_argument("-f", "--follow", type=inkex.Boolean, dest="follow", default=True,
                                     help="choose between wave or snake effect")
        self.arg_parser.add_argument("-s", "--stretch", type=inkex.Boolean, dest="stretch", default=True,
                                     help="repeat the path to fit deformer's length")
        self.arg_parser.add_argument("-p", "--space", type=float, dest="space", default=0.0)
        self.arg_parser.add_argument("-v", "--vertical", type=inkex.Boolean, dest="vertical", default=False,
                                     help="reference path is vertical")
        self.arg_parser.add_argument("-d", "--duplicate", type=inkex.Boolean, dest="duplicate", default=False,
                                     help="duplicate pattern before deformation")
        self.arg_parser.add_argument("-c", "--copymode", type=str, dest="copymode", default="clone",
                                     help="duplicate pattern before deformation")
        self.arg_parser.add_argument("--tab", type=str, dest="tab",
                                     help="The selected UI-tab when OK was pressed")

    def prepareSelectionList(self):
        # first selected->pattern, all but first selected-> skeletons
        pattern_node = self.svg.selected.pop()

        self.gNode = Group()
        pattern_node.getparent().append(self.gNode)

        if self.options.copymode == "copy":
            self.patternNode = pattern_node.duplicate()
        elif self.options.copymode == "clone":
            # TODO: allow 4th option: duplicate the first copy and clone the next ones.
            self.patternNode = self.gNode.add(Use())
            self.patternNode.href = pattern_node
        else:
            self.patternNode = pattern_node

        self.skeletons = self.svg.selected
        self.expand_clones(self.skeletons, True, False)
        self.objects_to_paths(self.skeletons, False)

    def lengthtotime(self, l):
        """
        Receives an arc length l, and returns the index of the segment in self.skelcomp
        containing the corresponding point, to gether with the position of the point on this segment.

        If the deformer is closed, do computations modulo the total length.
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

    def localTransformAt(self, s, follow=True):
        """
        receives a length, and returns the corresponding point and tangent of self.skelcomp
        if follow is set to false, returns only the translation
        """
        i, t = self.lengthtotime(s)
        if i == len(self.skelcomp) - 1:
            x, y = bezier.between_point(self.skelcomp[i - 1], self.skelcomp[i], 1 + t)
            dx = (self.skelcomp[i][0] - self.skelcomp[i - 1][0]) / self.lengths[-1]
            dy = (self.skelcomp[i][1] - self.skelcomp[i - 1][1]) / self.lengths[-1]
        else:
            x, y = bezier.between_point(self.skelcomp[i], self.skelcomp[i + 1], t)
            dx = (self.skelcomp[i + 1][0] - self.skelcomp[i][0]) / self.lengths[i]
            dy = (self.skelcomp[i + 1][1] - self.skelcomp[i][1]) / self.lengths[i]
        if follow:
            mat = [[dx, -dy, x], [dy, dx, y]]
        else:
            mat = [[1, 0, x], [0, 1, y]]
        return mat

    def effect(self):

        if len(self.options.ids) < 2:
            inkex.errormsg(_("This extension requires two selected paths."))
            return
        self.prepareSelectionList()

        # center at (0,0)
        bbox = self.patternNode.bounding_box()
        mat = [[1, 0, -bbox.center.x], [0, 1, -bbox.center.y]]
        if self.options.vertical:
            bbox = BoundingBox(-bbox.y, bbox.x)
            mat = (Transform([[0, -1, 0], [1, 0, 0]]) * Transform(mat)).matrix
        mat[1][2] += self.options.noffset
        self.patternNode.transform *= mat

        width = bbox.width
        dx = width + self.options.space

        # check if group and expand it
        patternList = []
        if self.options.grouppick and isinstance(self.patternNode, Group):
            mat = self.patternNode.transform
            for child in self.patternNode:
                child.transform *= mat
                patternList.append(child)
        else:
            patternList.append(self.patternNode)
        # inkex.debug(patternList)

        counter = 0
        for skelnode in self.skeletons.values():
            self.curSekeleton = skelnode.path.to_superpath()
            for comp in self.curSekeleton:
                self.skelcomp, self.lengths = linearize(comp)
                # !!!!>----> TODO: really test if path is closed! end point==start point is not enough!
                self.skelcompIsClosed = (self.skelcomp[0] == self.skelcomp[-1])

                length = sum(self.lengths)
                if self.options.stretch:
                    dx = width + self.options.space
                    n = int((length - self.options.toffset + self.options.space) / dx)
                    if n > 0:
                        dx = (length - self.options.toffset) / n

                xoffset = self.skelcomp[0][0] - bbox.x.minimum + self.options.toffset
                yoffset = self.skelcomp[0][1] - bbox.y.center - self.options.noffset

                s = self.options.toffset
                while s <= length:
                    mat = self.localTransformAt(s, self.options.follow)
                    if self.options.pickmode == "rand":
                        clone = copy.deepcopy(patternList[random.randint(0, len(patternList) - 1)])

                    if self.options.pickmode == "seq":
                        clone = copy.deepcopy(patternList[counter])
                        counter = (counter + 1) % len(patternList)

                    # !!!--> should it be given an id?
                    # seems to work without this!?!
                    myid = patternList[random.randint(0, len(patternList) - 1)].tag.split('}')[-1]
                    clone.set("id", self.svg.get_unique_id(myid))
                    self.gNode.append(clone)

                    clone.transform *= mat
                    s += dx

        self.patternNode.getparent().remove(self.patternNode)


if __name__ == '__main__':
    PathScatter().run()

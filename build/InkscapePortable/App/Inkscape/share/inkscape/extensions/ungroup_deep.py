#!/usr/bin/env python
# coding=utf-8
"""
see #inkscape on Freenode and
https://github.com/nikitakit/svg2sif/blob/master/synfig_prepare.py#L370
for an example how to do the transform of parent to children.
"""

import inkex
from inkex import (
    Group, Anchor, Switch, NamedView, Defs, Metadata, ForeignObject,
    ClipPath, Use, SvgDocumentElement,
)

class UngroupDeep(inkex.EffectExtension):
    def add_arguments(self, pars):
        pars.add_argument("--startdepth", type=int, default=0,
                          help="starting depth for ungrouping")
        pars.add_argument("--maxdepth", type=int, default=65535,
                          help="maximum ungrouping depth")
        pars.add_argument("--keepdepth", type=int, default=0,
                          help="levels of ungrouping to leave untouched")

    @staticmethod
    def _merge_style(node, style):
        """Propagate style and transform to remove inheritance
        Originally from
        https://github.com/nikitakit/svg2sif/blob/master/synfig_prepare.py#L370
        """

        # Compose the style attribs
        this_style = node.style
        remaining_style = {}  # Style attributes that are not propagated

        # Filters should remain on the top ancestor
        non_propagated = ["filter"]
        for key in non_propagated:
            if key in this_style.keys():
                remaining_style[key] = this_style[key]
                del this_style[key]

        # Create a copy of the parent style, and merge this style into it
        parent_style_copy = style.copy()
        parent_style_copy.update(this_style)
        this_style = parent_style_copy

        # Merge in any attributes outside of the style
        style_attribs = ["fill", "stroke"]
        for attrib in style_attribs:
            if node.get(attrib):
                this_style[attrib] = node.get(attrib)
                del node.attrib[attrib]

        if isinstance(node, (SvgDocumentElement, Anchor, Group, Switch)):
            # Leave only non-propagating style attributes
            if not remaining_style:
                if "style" in node.keys():
                    del node.attrib["style"]
            else:
                node.style = remaining_style

        else:
            # This element is not a container

            # Merge remaining_style into this_style
            this_style.update(remaining_style)

            # Set the element's style attribs
            node.style = this_style

    def _merge_clippath(self, node, clippathurl):

        if clippathurl:
            node_transform = node.transform
            if node_transform:
                # Clip-paths on nodes with a transform have the transform
                # applied to the clipPath as well, which we don't want.  So, we
                # create new clipPath element with references to all existing
                # clippath subelements, but with the inverse transform applied
                new_clippath = self.svg.defs.add(ClipPath(clipPathUnits='userSpaceOnUse'))
                new_clippath.set_random_id('clipPath')
                clippath = self.svg.getElementById(clippathurl[5:-1])
                for child in clippath.iterchildren():
                    use = new_clippath.add(Use())
                    use.add('xlink:href', '#' + child.get("id"))
                    use.transform = -node_transform
                    use.set_random_id('use')

                # Set the clippathurl to be the one with the inverse transform
                clippathurl = "url(#" + new_clippath.get("id") + ")"

            # Reference the parent clip-path to keep clipping intersection
            # Find end of clip-path chain and add reference there
            node_clippathurl = node.get("clip-path")
            while node_clippathurl:
                node = self.svg.getElementById(node_clippathurl[5:-1])
                node_clippathurl = node.get("clip-path")
            node.set("clip-path", clippathurl)

    # Flatten a group into same z-order as parent, propagating attribs
    def _ungroup(self, node):
        node_parent = node.getparent()
        node_index = list(node_parent).index(node)
        node_style = node.style

        node_transform = node.transform
        node_clippathurl = node.get('clip-path')
        for child in reversed(list(node)):

            child.transform *= node_transform

            if node.get("style") is not None:
                self._merge_style(child, node_style)
            self._merge_clippath(child, node_clippathurl)
            node_parent.insert(node_index, child)
        node_parent.remove(node)

    # Put all ungrouping restrictions here
    def _want_ungroup(self, node, depth, height):
        if (isinstance(node, Group) and
                node.getparent() is not None and
                height > self.options.keepdepth and
                self.options.startdepth <= depth <=
                self.options.maxdepth):
            return True
        return False

    def _deep_ungroup(self, node):
        # using iteration instead of recursion to avoid hitting Python
        # max recursion depth limits, which is a problem in converted PDFs

        # Seed the queue (stack) with initial node
        q = [{'node': node,
              'depth': 0,
              'prev': {'height': None},
              'height': None}]

        while q:
            current = q[-1]
            node = current['node']
            depth = current['depth']
            height = current['height']

            # Recursion path
            if height is None:
                # Don't enter non-graphical portions of the document
                if isinstance(node, (NamedView, Defs, Metadata, ForeignObject)):
                    q.pop()

                # Base case: Leaf node
                if not isinstance(node, Group) or not list(node):
                    current['height'] = 0

                # Recursive case: Group element with children
                else:
                    depth += 1
                    for child in node.iterchildren():
                        q.append({'node': child, 'prev': current,
                                  'depth': depth, 'height': None})

            # Return path
            else:
                # Ungroup if desired
                if self._want_ungroup(node, depth, height):
                    self._ungroup(node)

                # Propagate (max) height up the call chain
                height += 1
                previous = current['prev']
                prev_height = previous['height']
                if prev_height is None or prev_height < height:
                    previous['height'] = height

                # Only process each node once
                q.pop()

    def effect(self):
        if self.svg.selected:
            for node in self.svg.selected.values():
                self._deep_ungroup(node)
        else:
            for node in self.document.getroot():
                self._deep_ungroup(node)


if __name__ == '__main__':
    UngroupDeep().run()

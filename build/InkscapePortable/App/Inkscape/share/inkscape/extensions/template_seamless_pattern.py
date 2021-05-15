#!/usr/bin/env python
# coding=utf-8

# Written by Jabiertxof
# V.06

import os

import inkex
from inkex import load_svg

class SeamlessPattern(inkex.TemplateExtension):
    """Generate a seamless pattern template"""
    multi_inx = True

    @classmethod
    def get_template(cls):
        name = "seamless_pattern.svg"
        path = os.path.dirname(os.path.realpath(__file__))
        return load_svg(os.path.join(path, name))

    def set_namedview(self, width, height, unit):
        width = self.options.width
        height = self.options.height
        factor = width / height
        clip_rect = self.svg.getElementById("clipPathRect")
        clip_rect.set("width", str(width))
        clip_rect.set("height", str(height))

        scale = inkex.Transform(scale=(width / 100, height / 100))
        self.svg.getElementById("designTop").transform = scale
        self.svg.getElementById("designBottom").transform = scale

        scale = (1, factor) if factor <= 1 else (1 / factor, 1)
        for child in self.svg.getElementById("designTop"):
            child.transform = inkex.Transform(scale=scale)

        text_preview = self.svg.getElementById('textPreview')
        if text_preview is not None:
            x = width / 100.0 / factor
            y = height / 1000.0
            if factor <= 1:
                x *= factor
                y *= factor
            text_preview.transform = inkex.Transform(translate=(int(width) * 2, 0), scale=(x, y))

        info_group = self.svg.getElementById('infoGroup')
        if info_group is not None:
            scale = 100 if factor <= 1 else 1000
            info_group.transform = inkex.Transform(scale=(width / scale, height / scale * factor))

        sides = [(x, y) for y in (-height, 0, height) for x in (-width, 0, width)]
        for i, (x, y) in enumerate(sides):
            top = self.svg.getElementById('top{i}'.format(i=i+1))
            bottom = self.svg.getElementById('bottom{i}'.format(i=i+1))
            if top is not None and bottom is not None:
                bottom.transform = top.transform = inkex.Transform(translate=(x, y))

        clones = [(x, y) for x in (0, width, width * 2) for y in (0, height, height * 2)]
        for i, (x, y) in enumerate(clones):
            preview = self.svg.getElementById("clonePreview{i}".format(i=i))
            if preview is not None:
                preview.transform = inkex.Transform(translate=(x, y))

        pattern_generator = self.svg.getElementById('fullPatternClone')
        if pattern_generator is not None:
            pattern_generator.transform = inkex.Transform(translate=(width * 2, -height))
            pattern_generator.set("inkscape:tile-cx", width / 2)
            pattern_generator.set("inkscape:tile-cy", height / 2)
            pattern_generator.set("inkscape:tile-w", width)
            pattern_generator.set("inkscape:tile-h", height)
            pattern_generator.set("inkscape:tile-x0", width)
            pattern_generator.set("inkscape:tile-y0", height)
            pattern_generator.set("width", width)
            pattern_generator.set("height", height)

        namedview = self.svg.namedview
        namedview.set('inkscape:document-units', 'px')
        namedview.set('inkscape:cx', (width * 5.5) / 2)
        namedview.set('inkscape:cy', "0")
        namedview.set('inkscape:zoom', 1 / (width / 100))

if __name__ == '__main__':
    SeamlessPattern().run()

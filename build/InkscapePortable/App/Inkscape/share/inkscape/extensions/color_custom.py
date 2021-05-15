#!/usr/bin/env python

import ast
import operator as op
import inkex

OPS = {ast.Add: op.add,
       ast.Sub: op.sub,
       ast.Mult: op.mul,
       ast.Div: op.truediv,
       ast.Pow: op.pow,
       ast.BitXor: op.xor,
       ast.USub: op.neg}

def eval_expr(expr, namespace):
    """
    >>> eval_expr('2^6')
    4
    >>> eval_expr('2**6')
    64
    >>> eval_expr('1 + 2*3**(4^5) / (6 + -7)')
    -5.0
    """
    return _eval(ast.parse(expr, mode='eval').body, namespace)

def _eval(node, namespace):
    if isinstance(node, ast.Num):  # <number>
        return node.n
    elif isinstance(node, ast.Name):  # <variable> (must be in namespace)
        return namespace[node.id]
    elif isinstance(node, ast.BinOp):  # <left> <operator> <right>
        return OPS[type(node.op)](_eval(node.left, namespace), _eval(node.right, namespace))
    elif isinstance(node, ast.UnaryOp):  # <operator> <operand> e.g., -1
        return OPS[type(node.op)](_eval(node.operand, namespace))
    else:
        raise TypeError(node)

class Custom(inkex.ColorExtension):
    """Custom colour functions per channel"""
    def add_arguments(self, pars):
        pars.add_argument("--tab")
        pars.add_argument("-r", "--r", default="r", help="red channel function")
        pars.add_argument("-g", "--g", default="g", help="green channel function")
        pars.add_argument("-b", "--b", default="b", help="blue channel function")
        pars.add_argument("-s", "--scale", type=float, default=1.0, help="The input (r,g,b) range")

    def modify_color(self, name, color):
        opt = self.options
        factor = 255.0 / opt.scale
        # add stuff to be accessible from within the custom color function here.
        namespace = {
            'r': float(color.red) / factor,
            'g': float(color.green) / factor,
            'b': float(color.blue) / factor}

        color.red = int(eval_expr(opt.r.strip(), namespace) * factor)
        color.green = int(eval_expr(opt.g.strip(), namespace) * factor)
        color.blue = int(eval_expr(opt.b.strip(), namespace) * factor)
        return color

if __name__ == '__main__':
    Custom().run()

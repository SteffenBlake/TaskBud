/*!
 * Released under the MIT license
 * Copyright (c) 2020 Steffen Cole Blake other contributors
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * Date: 2020-06-08T23:31Z
 */

(function ($) {
    var onDragMove = function(event, ui, settings) {
        var $this = $(this);

        var width = parseFloat($this.css("width"));
        var current_p = (ui.position.left / width);
        var target_p = Math.min(Math.max(current_p, -settings.leftMax), settings.rightMax);
        var targetLeft = Math.round(width * target_p);
        ui.position.left = targetLeft;
    }

    var onDragStop = function(event, ui, settings) {
        var $this = $(this);
        $this.removeClass("dragging");

        var width = parseFloat($this.css("width"));
        var current_p = (ui.position.left / width);

        if (current_p <= -settings.leftMinTrigger) {
            settings.left(this);
            ui.position.left = 0;
        } else if (current_p >= settings.rightMinTrigger) {
            settings.right(this);
            ui.position.left = 0;
        }
    }

    var swipeable = function(settings) {
        settings.axis = "x";
        settings.revert = true;
        settings.revertDuration = settings.revertDuration || 200;
        settings.drag = (event, ui) => onDragMove.call(event.target, event, ui, settings);
        settings.stop = (event, ui) => onDragStop.call(event.target, event, ui, settings);

        $(this).draggable(settings);
    }

    $.fn.swipeable = swipeable;
})(jQuery);
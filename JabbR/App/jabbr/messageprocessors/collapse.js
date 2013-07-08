﻿/*global define*/
define([
    'logger',
    'kernel',
    'jabbr/events'
], function (Logger, kernel, events) {
    var logger = new Logger('jabbr/messageprocessors/collapse');
    logger.trace('loaded');

    var processor = null,
        ru = null,
        rc = null;

    function process(content, roomName) {
        if (shouldCollapseContent(content, roomName)) {
            return collapseRichContent(content);
        }

        return content;
    }

    function isFromCollapsibleContentProvider(content) {
        return content.indexOf('class="collapsible_box') > -1; // leaving off trailing " purposefully
    }

    function shouldCollapseContent(content, roomName) {
        var collapsible = isFromCollapsibleContentProvider(content),
            collapseForRoom = roomName ?
                rc.getRoomPreference(roomName, 'blockRichness') :
                ru.getActiveRoomPreference('blockRichness');

        return collapsible && collapseForRoom;
    }

    function collapseRichContent(content) {
        content = content.replace(/class="collapsible_box/g, 'style="display: none;" class="collapsible_box');
        return content.replace(/class="collapsible_title"/g, 'class="collapsible_title" title="Content collapsed because you have Rich-Content disabled"');
    }

    return {
        initialize: function () {
            processor = kernel.get('jabbr/messageprocessors/processor');
            ru = kernel.get('jabbr/components/rooms.ui');
            rc = kernel.get('jabbr/components/rooms.client');
            logger.trace('initialized');
        },

        process: process,

        bind: function () {
            processor.bind(events.processor.beforeProcessRichContent, function (event, handler) {
                handler.set(process(handler.get(), handler.data.roomName));
            });
        }
    };
});
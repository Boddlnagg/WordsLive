var sourceActive = false;
var copyrightActive = false;
var inAnimation = false;
var inBackgroundAnimation = false;

function updateSlide(html) {
    $("#current").html(html);
}

function showCopyright(show) {
    if (show) {
        $("#copyright").fadeIn(0);
    }
    else {
        $("#copyright").fadeOut(0);
    }
    copyrightActive = show;
}

function showSource(show) {
    if (show) {
        $("#source").fadeIn(0);
    }
    else {
        $("#source").fadeOut(0);
    }
    sourceActive = show;
}

function setCopyright(html) {

    $("#copyright").html(html);
}

function setSource(html) {

    $("#source").html(html);
}

function changeBackground(bg, fadeTime) {
    if (inBackgroundAnimation) {
        $("#next-background").remove();
    }
    inBackgroundAnimation = true;

    $("#current-background").before('<div id="next-background"><div class="main" style="' + bg + '"></div></div>');
    $("#current-background").fadeOut(fadeTime, 'linear', function () {
        $("#current-background").remove();
        $("#next-background").attr("id", "current-background");
        inBackgroundAnimation = false;
    });
}

function updateCss(css) {
    $().ready(function () {
        $("#style").html(css);
    });
}

function preloadImages(paths) {
    $().ready(function () {
        $.each(paths, function(i) {
            var img = new Image();
            img.src = paths[i];
        });
        callback.imagesLoaded();
    });
}
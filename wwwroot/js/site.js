var linkString = '';
function linkMaterial(mat, id) {
    linkString += ':'+mat;
    $("#linksinput")[0].value = linkString.slice(1);
    let li = $("<li/>").appendTo($("#links"));
    $("<a/>", { href: "/Materials/At?name=" + mat, html: mat }).appendTo(li);
    document.getElementById('mat' + id).remove();
}
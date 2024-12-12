document.getElementById('imageUpload').addEventListener('change', function (event) {
    var preview = document.getElementById('imagePreview');
    preview.innerHTML = '';  // Clear previous previews

    for (var i = 0; i < event.target.files.length; i++) {
        var file = event.target.files[i];
        var reader = new FileReader();

        reader.onload = function (e) {
            var imageContainer = document.createElement('div');
            imageContainer.classList.add('image-container');
            var img = document.createElement('img');
            img.src = e.target.result;
            imageContainer.appendChild(img);
            preview.appendChild(imageContainer);
        }

        reader.readAsDataURL(file);
    }
});

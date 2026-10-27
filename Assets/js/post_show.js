const postEditLink = document.querySelector("#post-edit-link");
const postEditForm = document.querySelector("#post-edit-form");
const tagBox = document.querySelector("#tag-string");

postEditLink.addEventListener("click", () =>
{
    postEditForm.style.display = "block";
    setTimeout(() =>
    {
        tagBox.focus();
        const textLength = tagBox.value.length;
        tagBox.setSelectionRange(textLength, textLength);
    }, 10);
});

const postEditLink = document.querySelector("#post-edit-link");
const postEditForm = document.querySelector("#post-edit-form");
const tagBox = document.querySelector("#tag-string");

const addCommentaryLink = document.querySelector("#add-commentary-link");
const addCommentaryForm = document.querySelector("#add-commentary-form");

postEditLink.addEventListener("click", () =>
{
    postEditForm.style.display = "block";
    addCommentaryForm.style.display = "none";
    setTimeout(() =>
    {
        tagBox.focus();
        const textLength = tagBox.value.length;
        tagBox.setSelectionRange(textLength, textLength);
    }, 10);
});

addCommentaryLink.addEventListener("click", () =>
{
    postEditForm.style.display = "none";
    addCommentaryForm.style.display = "block";
});
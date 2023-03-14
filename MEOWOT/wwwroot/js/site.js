let labels = document.getElementsByTagName("label");
let inputs = document.getElementsByClassName("sus");
//form styling end
//modal form functionality
let modalForm = document.getElementById("modal-form");
let openModalButton = document.getElementById("add-btn");
let closeModalButton = document.getElementById("close-btn");
let pageContent = document.getElementById("page-content");
//modal form end

//lightbox??
const popups = document.getElementsByClassName("popup");
let popupImages = document.getElementsByClassName("popup-image");
const openImages = document.getElementsByClassName("card-image-container");
const openButtons = document.getElementsByClassName("open");

let left = document.getElementsByClassName("left");
let right = document.getElementsByClassName("right");

const observer = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
        if (entry.isIntersecting) {
            entry.target.classList.add('show');
        }
        else {
            entry.target.classList.remove('show');
        }
    });
});
const hiddenElements = document.querySelectorAll('.hidden');
labels = document.getElementsByTagName("label");

function OpenImage(i) {
    console.log(i);
    popups[i].classList.remove("popup-hide");
    popups[i].style.display = "flex";
    if (popupImages[i].clientWidth < popupImages[i].clientHeight) {
        popupImages[i].classList.add("vertical");
    }
}

window.onload = (event) => {
    for (var i = 0; i < inputs.length; i++) {
        let j = i;
        if (inputs[i].value != "") {
            labels[i].style.marginBottom = "2em";
        }
        inputs[i].addEventListener("focusin", e => {
            labels[j].style.marginBottom = "2em";
        })
        inputs[i].addEventListener("focusout", e => {
            if (inputs[j].value == "") {
                labels[j].style.marginBottom = "0";
            }
        })
    }

    for (let i = 0; i < openButtons.length; i++) {
        openButtons[i].addEventListener("click", OpenImage.bind(this, i));
        openImages[i].addEventListener("click", OpenImage.bind(this, i));
    }

    Array.from(popups).forEach(e => {
        e.addEventListener("click", el => {
            for (var i = 0; i < popups.length; i++) {
                popups[i].classList.add("popup-hide");
            }
        });
    });




};
function InitLabels() {
    labels = document.getElementsByTagName("label");
}

hiddenElements.forEach((el) => observer.observe(el));

openModalButton.addEventListener("click", e => {
    modalForm.classList.add("form-active");
    pageContent.classList.add("fucked");
})
closeModalButton.addEventListener("click", e => {
    modalForm.classList.remove("form-active");
    pageContent.classList.remove("fucked");
})
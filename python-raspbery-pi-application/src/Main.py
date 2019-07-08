import sys

from PyQt5.QtWidgets import QWidget, QLabel, QApplication, QPushButton, QHBoxLayout, QVBoxLayout, QComboBox, QMainWindow
from CalenderSelect import Select


def main():
    app = QApplication(sys.argv)
    start = Select()
    sys.exit(app.exec_())


if __name__ == '__main__':
    main()
